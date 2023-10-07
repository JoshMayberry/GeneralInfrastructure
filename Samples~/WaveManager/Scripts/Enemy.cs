using UnityEngine;
using UnityEngine.Splines;
using jmayberry.GeneralInfrastructure.Spawner;

namespace jmayberry.GeneralInfrastructure.Samples {

    public class Enemy : MonoBehaviour, ISpawnable {
        SplineAnimate splineAnim;
        public int health = 2;
        private int healthMax;

        private UnitySpawner<Enemy> spawner;

        void Awake() {
            this.healthMax = this.health;
            this.splineAnim = this.GetComponent<SplineAnimate>();
        }

        void Reset() {
            this.health = this.healthMax;
        }

        internal void WalkPath(SplineContainer path) {
            splineAnim.Container = path;
            splineAnim.Play();
        }

        public void Damage(int amount) {
            this.health -= amount;
            if (health <= 0f) {
                this.Die();
            }
        }

        public void Die() {
            this.spawner.Despawn(this);
        }

        public void OnSpawn(object spawner) {
            if (spawner is UnitySpawner<Enemy> enemySpawner) {
                this.spawner = enemySpawner;
            }

            this.Reset();
        }

        public void OnDespawn(object spawner) {
        }
    }
}