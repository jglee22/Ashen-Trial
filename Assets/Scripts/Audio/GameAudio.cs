using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class GameAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip uiClick;
        [SerializeField] private AudioClip uiConfirm;
        [SerializeField] private AudioClip gameOverSting;
        [SerializeField] private AudioClip runCompleteSting;
        [SerializeField] private AudioClip[] playerSwings;
        [SerializeField] private AudioClip playerDodge;
        [SerializeField] private AudioClip playerDamage;
        [SerializeField] private AudioClip bossHit;
        [SerializeField] private AudioClip bossMeleeSwing;
        [SerializeField] private AudioClip bossCharge;
        [SerializeField] private AudioClip bossAoE;
        [SerializeField] private AudioClip magicCast;
        [SerializeField] private AudioClip magicBurst;
        [SerializeField] private AudioClip bossDash;
        [SerializeField] private AudioClip sequentialBurst;
        [SerializeField] private AudioClip bossDeath;

        public void PlayUiClick() => PlayUi(uiClick);
        public void PlayUiConfirm() => PlayUi(uiConfirm);
        public void PlayGameOver() => PlayUi(gameOverSting);
        public void PlayRunComplete() => PlayUi(runCompleteSting);

        public void PlayPlayerSwing(int comboStep)
        {
            if (playerSwings == null || playerSwings.Length == 0) return;
            int index = comboStep - 1;
            if (index < 0) index = 0;
            if (index >= playerSwings.Length) index = playerSwings.Length - 1;
            PlaySfx(playerSwings[index]);
        }

        public void PlayDodge() => PlaySfx(playerDodge);
        public void PlayPlayerDamage() => PlaySfx(playerDamage);
        public void PlayBossHit() => PlaySfx(bossHit);
        public void PlayBossMeleeSwing() => PlaySfx(bossMeleeSwing);
        public void PlayBossCharge() => PlaySfx(bossCharge);
        public void PlayBossAoE() => PlaySfx(bossAoE);
        public void PlayMagicCast() => PlaySfx(magicCast);
        public void PlayMagicBurst() => PlaySfx(magicBurst);
        public void PlayBossDash() => PlaySfx(bossDash);
        public void PlaySequentialBurst() => PlaySfx(sequentialBurst);
        public void PlayBossDeath() => PlaySfx(bossDeath);

        private void PlayUi(AudioClip clip)
        {
            if (uiSource == null || clip == null) return;
            uiSource.PlayOneShot(clip);
        }

        private void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }
    }
}
