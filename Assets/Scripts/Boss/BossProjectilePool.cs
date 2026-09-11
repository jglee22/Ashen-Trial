using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class BossProjectilePool : MonoBehaviour
    {
        [SerializeField] private BossProjectile projectilePrefab;
        [SerializeField, Min(1)] private int initialCapacity = 10;
        [SerializeField, Min(1)] private int maxSize = 20;
        private ObjectPool<BossProjectile> pool;
        private readonly List<BossProjectile> activeProjectiles = new List<BossProjectile>();

        public float ProjectileWidth => projectilePrefab.GetComponent<SphereCollider>().radius *
            projectilePrefab.transform.localScale.x * 2f;

        public BossProjectile Spawn(Vector3 position, Vector3 direction, float speed, float damage,
            float lifetime, Health target, CharacterController targetBody)
        {
            if (pool == null)
            {
                if (projectilePrefab == null)
                {
                    Debug.LogError("BossProjectilePool: Projectile prefab is required.", this);
                    return null;
                }
                pool = new ObjectPool<BossProjectile>(CreateProjectile, activeProjectiles.Add,
                    projectile => { activeProjectiles.Remove(projectile); }, DestroyProjectile,
                    true, Mathf.Max(1, initialCapacity), Mathf.Max(1, maxSize));
            }
            BossProjectile projectile = pool.Get();
            projectile.Launch(position, direction, speed, damage, lifetime, target, targetBody, pool);
            return projectile;
        }

        private BossProjectile CreateProjectile()
        {
            BossProjectile projectile = Instantiate(projectilePrefab, transform);
            projectile.gameObject.SetActive(false);
            return projectile;
        }

        public void ReleaseAllActive()
        {
            while (activeProjectiles.Count > 0)
                activeProjectiles[activeProjectiles.Count - 1].Release();
        }

        private void DestroyProjectile(BossProjectile projectile)
        {
            if (projectile != null) Destroy(projectile.gameObject);
        }

        private void OnDestroy()
        {
            activeProjectiles.Clear();
            pool?.Dispose();
        }
    }
}
