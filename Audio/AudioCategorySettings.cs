using UnityEngine;

namespace Dev.Audio
{
    public class AudioCategorySettings
    {
        // Misc
        public readonly AudioCategory Category;
        public readonly string Name;
        private readonly string playerPrefsKey;
        private bool hasChangedFromDefault;
        
        private float defaultVolume;
        public float DefaultVolume
        {
            get => defaultVolume;
            set => SetDefaultVolume(value);
        }

        // Volume
        private float volume = 1f;
        public float Volume
        {
            get => GetVolume();
            set => SetVolume(value);
        }

        // Events
        public delegate void VolumeChangeHandler(AudioCategorySettings settings, float prevVolume, float newVolume);
        public event VolumeChangeHandler OnVolumeChanged;

        public AudioCategorySettings(AudioCategory category)
            : this(category, category.ToString().ToLowerInvariant())
        {
            
        }

        public AudioCategorySettings(AudioCategory category, string name, float defaultVolume = 1f)
        {
            Category = category;
            Name = name;
            this.defaultVolume = defaultVolume;
            
            playerPrefsKey = "Volume_" + Category;
        }

        protected virtual float GetVolume()
        {
            return volume;
        }

        protected virtual void SetVolume(float newVolume, bool ignoreDirty = false)
        {
            newVolume = Mathf.Clamp01(newVolume);
            if(Mathf.Approximately(newVolume, volume))
            {
                return;
            }
            
            var prevVolume = volume;
            volume = newVolume;

            if(!ignoreDirty)
            {
                hasChangedFromDefault = true;
            }

            OnVolumeChanged?.Invoke(this, prevVolume, volume);
        }

        private void SetDefaultVolume(float newDefaultVolume)
        {
            defaultVolume = newDefaultVolume;
            
            // We should only change the current volume to the new default volume if the player
            // has not altered the volume
            if(hasChangedFromDefault)
            {
                return;
            }
            
            SetVolume(defaultVolume, true);
        }

        public virtual void ReApplyVolume()
        {
            SetVolume(volume);
        }

        public virtual void Save()
        {
            if(!hasChangedFromDefault)
            {
                return;
            }
            
            PlayerPrefs.SetFloat(playerPrefsKey, volume);
        }

        public virtual void Load()
        {
            if(!PlayerPrefs.HasKey(playerPrefsKey))
            {
                SetVolume(1f, true);
                return;
            }
            
            var newVolume = PlayerPrefs.GetFloat(playerPrefsKey, 1f);
            SetVolume(newVolume);
        }
    }
}