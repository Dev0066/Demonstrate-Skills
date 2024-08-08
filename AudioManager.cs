using System;
using System.Collections.Generic;

using Machete.Core;
using Machete.Tools.Pooling;

using UnityEngine;

namespace Machete.Audio
{
	[RequireComponent(typeof(AudioSource))]
	public class AudioManager : Manager<AudioManager>
	{
		// Settings
		[Header("Settings")]
		public AudioClip ButtonClickClip;
		public PoolingProfile PoolingProfile = new PoolingProfile(20, true);
		
		// Misc
		public AudioSource AudioSource { get; private set; }
	    private AudioSourcePool pool;
	    public AudioSourcePool Pool => pool;

	    public static bool IsSoundEnabled
		{
			get => GlobalVolume > 0f;
			set => GlobalVolume = value ? 1f : 0f;
		}

        // Volumes
	    private AudioCategorySettings[] categorySettings;
	    private Dictionary<string, AudioCategorySettings> categorySettingsByName;

	    public static float GlobalVolume
	    {
	        get => GetVolume(AudioCategory.Global);
	        set => SetVolume(AudioCategory.Global, value);
	    }

	    public static float FxVolume
	    {
	        get => GetVolume(AudioCategory.Fx);
	        set => SetVolume(AudioCategory.Fx, value);
	    }

	    public static float SpeechVolume
	    {
	        get => GetVolume(AudioCategory.Speech);
	        set => SetVolume(AudioCategory.Speech, value);
	    }

	    public static float MusicVolume
	    {
	        get => GetVolume(AudioCategory.Music);
	        set => SetVolume(AudioCategory.Music, value);
	    }
	    
	    // Volume listeners
	    private readonly LinkedList<IVolumeListener> volumeListeners = new LinkedList<IVolumeListener>();
	    private readonly Stack<LinkedListNode<IVolumeListener>> volumeListenerNodePool
		    = new Stack<LinkedListNode<IVolumeListener>>();

        // Events
	    public delegate void VolumeChangeHandler(AudioCategory category, float prevVolume, float newVolume);
	    public static event VolumeChangeHandler OnVolumeChanged;

        protected override void OnAwakeManager()
		{
			LoadCategories();

            // Mute the sound if we're in headless mode. Sometimes sound seems to be played
            // in batch mode on certain computers.
	        if(MacheteCore.IsHeadlessModeEnabled)
	        {
	            AudioListener.volume = 0f;
	        }
	        
	        // Create the audio source pool
	        PoolingProfile.PersistThroughSceneChanges = true;
	        pool = new AudioSourcePool(PoolingProfile, null);
	    }

	    protected AudioCategorySettings CreateSettings(AudioCategory category)
        {
            switch(category)
            {
                case AudioCategory.Global:
                    return new GlobalCategorySettings();
                default:
                    return new AudioCategorySettings(category);
            }
        }

        #region Audio categories
	    
        [ContextMenu("Load audio categories")]
	    protected virtual void LoadCategories()
	    {
	        var categories = Enum.GetValues(typeof(AudioCategory));

            // Create settings collections
            categorySettings = new AudioCategorySettings[categories.Length];
            categorySettingsByName = new Dictionary<string, AudioCategorySettings>();

            // Load categories
	        for(var i = 0; i < categories.Length; i++)
	        {
                var category = (AudioCategory)categories.GetValue(i);

                // Create category settings
	            var settings = CreateSettings(category);
                AddCategorySettings(settings);

                // Load the settings
                settings.Load();
	        }
	    }

	    [ContextMenu("Save audio categories")]
	    protected virtual void SaveCategories(bool savePlayerPrefs = true)
        {
            for(var i = 0; i < categorySettings.Length; i++)
            {
                var settings = categorySettings[i];
                if(settings == null)
                {
                    continue;
                }

                settings.Save();
            }

            if(savePlayerPrefs)
            {
				PlayerPrefs.Save();   
            }
        }
	    
	    protected AudioCategorySettings GetCategorySettings(int index)
	    {
		    if(index < 0 || index > Instance.categorySettings.Length)
		    {
			    return null;
		    }

		    return Instance.categorySettings[index];
	    }

	    public AudioCategorySettings GetCategorySettings(AudioCategory category)
	    {
		    return GetCategorySettings((int)category);
	    }

	    protected AudioCategorySettings GetCategorySettings(string categoryName)
	    {
		    return categorySettingsByName.TryGetValue(categoryName, out var settings) ? settings : null;
	    }

	    protected void AddCategorySettings(AudioCategorySettings settings)
	    {
		    var index = (int)settings.Category;

		    // Expand settings array if required
		    if(index > categorySettings.Length - 1)
		    {
			    var newSize = Mathf.Max(index + 1, categorySettings.Length * 2);
			    var newArray = new AudioCategorySettings[newSize];
			    Array.Copy(categorySettings, 0, newArray, 0, categorySettings.Length);
			    categorySettings = newArray;
		    }

		    // Set the category settings
		    categorySettings[index] = settings;
		    categorySettingsByName[settings.Name] = settings;

		    // Bind volume change event
		    settings.OnVolumeChanged += OnCategoryVolumeChanged;
	    }
	    
	    #endregion
	    
	    #region Volumes
	    
	    public static void SaveVolumes(bool savePlayerPrefs = true)
	    {
		    Instance.SaveCategories(savePlayerPrefs);
	    }
	    
	    public static float GetVolume(AudioCategory category)
	    {
		    var settings = Instance.GetCategorySettings(category);
		    return settings?.Volume ?? 0f;
	    }

	    public static void SetVolume(AudioCategory category, float newVolume)
	    {
		    var settings = Instance.GetCategorySettings(category);
		    if(settings == null)
		    {
			    return;
		    }

		    settings.Volume = newVolume;
	    }

	    public static void SetDefaultVolume(AudioCategory category, float defaultVolume)
	    {
		    var settings = Instance.GetCategorySettings(category);
		    settings.DefaultVolume = defaultVolume;
	    }

	    public static void ReApplyVolumes()
	    {
		    var categorySettings = Instance.categorySettings;
		    for(var i = 0; i < categorySettings.Length; i++)
		    {
			    var setting = categorySettings[i];
			    setting.ReApplyVolume();
		    }
	    }

	    private void UpdateVolumeListeners(AudioCategory category, float prevVolume, float newVolume)
	    {
		    var node = volumeListeners.First;
		    while(node != null)
		    {
			    var volumeListener = node.Value;
			    node = node.Next;

			    if(volumeListener == null)
			    {
				    Debug.LogWarning("Encountered a null volume listener.");
				    continue;
			    }

			    try
			    {
				    volumeListener.OnVolumeChanged(category, prevVolume, newVolume);
			    }
			    catch(Exception exception)
			    {
				    Debug.LogException(exception);
			    }
		    }
	    }

	    public void AddVolumeListener(IVolumeListener volumeListener)
	    {
		    var node = volumeListener.RegistryNode;
		    if(node != null)
		    {
			    return;
		    }

		    node = GetFreeVolumeListenerNode(volumeListener);
		    volumeListener.RegistryNode = node;
		    volumeListeners.AddLast(node);
	    }

	    public void RemoveVolumeListener(IVolumeListener volumeListener)
	    {
		    var node = volumeListener.RegistryNode;
		    if(node == null)
		    {
			    return;
		    }

		    // Remove the volume listener
		    volumeListeners.Remove(node);
		    volumeListener.RegistryNode = null;

		    // Recycle the node
		    node.Value = null;
		    volumeListenerNodePool.Push(node);
	    }

	    private LinkedListNode<IVolumeListener> GetFreeVolumeListenerNode(IVolumeListener volumeListener)
	    {
		    LinkedListNode<IVolumeListener> node;
		    if(volumeListenerNodePool.Count > 0)
		    {
			    node = volumeListenerNodePool.Pop();
			    node.Value = volumeListener;
		    }
		    else
		    {
			    node = new LinkedListNode<IVolumeListener>(volumeListener);
		    }

		    return node;
	    }

	    #endregion

	    #region Clip playing

	    public AudioSource GetFreeAudioSource()
	    {
		    return pool.GetNextComponent();
	    }

	    public AudioSourceWrapper GetFreeAudioSourceWrapper()
	    {
		    return pool.GetNextWrapper();
	    }
	    
        private static AudioSource PlayClip(AudioClip clip, bool loop, float spatialBlend)
        {
	        if(clip == null)
	        {
		        return null;
	        }
	        
	        // Fetch a free audio source wrapper
	        var audioSourceWrapper = Instance.GetFreeAudioSourceWrapper();
	        
	        // Reset the audio category to global
	        var audioCategorizer = audioSourceWrapper.AudioCategorizer;
	        audioCategorizer.Category = AudioCategory.Global;

            // Configure the audio source
            var audioSource = audioSourceWrapper.Component;
            audioSource.pitch = 1f;
            audioSource.spatialBlend = spatialBlend;
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();

            return audioSource;
        }

        public static AudioSource PlayClip2D(AudioClip clip, bool loop)
	    {
		    if(clip == null)
		    {
			    return null;
		    }
		    
	        var audioSource = PlayClip(clip, loop, 0f);
	        audioSource.spatialBlend = 0f;
	        return audioSource;
	    }

	    public static void PlayClip2D(AudioClip clip, float volumeScale = 1f)
        {
	        if(clip == null)
	        {
		        return;
	        }
	        
            Instance.AudioSource.PlayOneShot(clip, volumeScale);
        }

        public static AudioSource PlayClip3D(AudioClip clip, Vector3 position, bool loop)
        {
	        if(clip == null)
	        {
		        return null;
	        }
	        
            return PlayClip3D(clip, position, loop, 5f, 500f);
        }

        public static AudioSource PlayClip3D(AudioClip clip, Vector3 position, bool loop, float minDistance,
	        float maxDistance)
	    {
		    if(clip == null)
		    {
			    return null;
		    }
		    
            var audioSource = PlayClip(clip, loop, 1f);
            audioSource.transform.position = position;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            return audioSource;
        }
        
        #endregion
	    
	    #region Events

	    protected override void OnDestroyManager()
	    {
		    SaveCategories();
	    }

	    protected override void OnSceneChanged()
	    {
		    // Cache components
		    AudioSource = GetComponent<AudioSource>();
		    AudioSource.spatialBlend = 0f; // We only want 2D sounds for the main audio source
	    }

	    private void OnCategoryVolumeChanged(AudioCategorySettings settings, float prevVolume, float newVolume)
	    {
		    // Update our volume listeners
		    var audioCategory = settings.Category;
		    UpdateVolumeListeners(audioCategory, prevVolume, newVolume);
		    
		    // Fire volume changed event
		    OnVolumeChanged?.Invoke(audioCategory, prevVolume, newVolume);
	    }

		#endregion

		#region Manager related

		protected internal override bool IsPersistentManager()
		{
			return true;
		}

		protected internal override bool HasInitialization()
		{
			return false;
		}

        #endregion
    }
}