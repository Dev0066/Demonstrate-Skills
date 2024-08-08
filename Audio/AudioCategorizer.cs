using System.Collections.Generic;
using Machete.Core;
using UnityEngine;

namespace Dev.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioCategorizer : MonoBehaviour, IVolumeListener
    {
        // Components
        private AudioSource audioSource;
        public AudioSource AudioSource => audioSource;

        // Misc
        public LinkedListNode<IVolumeListener> RegistryNode { get; set; }
        private float unscaledVolume;
        public float UnscaledVolume
        {
            get => unscaledVolume;
            set => SetUnscaledVolume(value);
        }

        [SerializeField]
        private AudioCategory category;
        public AudioCategory Category
        {
            get => category;
            set => SetCategory(value);
        }

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            unscaledVolume = audioSource.volume;
        }

        private void UpdateVolume()
        {
            // Ignore the global category because it is already governed by the audio listener volume
            if(category == AudioCategory.Global)
            {
                AudioSource.volume = unscaledVolume;
                return;
            }
            
            AudioSource.volume = Mathf.Clamp01(unscaledVolume * AudioManager.GetVolume(category));
        }

        protected virtual void SetCategory(AudioCategory newCategory)
        {
            category = newCategory;
            UpdateVolume();
        }
        
        private void SetUnscaledVolume(float newUnscaledVolume)
        {
            unscaledVolume = newUnscaledVolume;
            UpdateVolume();
        }

        #region Events

        protected virtual void OnEnable()
        {
            // Subscribe to the audio manager as a volume listener
            AudioManager.Instance.AddVolumeListener(this);

            // Set the initial volume
            UpdateVolume();
        }

        protected virtual void OnDisable()
        {
            if(MacheteCore.IsShuttingDown)
            {
                return;
            }
            
            // Unsubscribe from the audio manager
            AudioManager.Instance.RemoveVolumeListener(this);
        }
        
        public void OnVolumeChanged(AudioCategory audioCategory, float oldVolume, float newVolume)
        {
            if(audioCategory != category)
            {
                return;
            }

            AudioSource.volume = newVolume;
        }

        #endregion
    }
}