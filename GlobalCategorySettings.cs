using UnityEngine;

namespace Machete.Audio
{
    public class GlobalCategorySettings : AudioCategorySettings
    {
        public GlobalCategorySettings()
            : base(AudioCategory.Global)
        {
            
        }

        public override void Load()
        {
            base.Load();

            AudioListener.volume = Volume;
        }

        protected override void SetVolume(float newVolume, bool ignoreDirty = false)
        {
            base.SetVolume(newVolume, ignoreDirty);

            AudioListener.volume = Volume;
        }
    }
}