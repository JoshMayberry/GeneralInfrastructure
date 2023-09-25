using UnityEngine;
using System.Collections.Generic;

namespace jmayberry.GeneralInfrastructure.Spawner {

	/**
	 * Example OnSpawn:
	 *  public void SomeMethod(object spawner) {
	 *      if (spawner is UnitySpawner<EnemyBase> mySpawner) {
	 *          this.mySpawner = mySpawner
	 *      }
	 *  }
	 */
	public interface ISpawnable {
		void OnSpawn(object spawner);
		void OnDespawn(object spawner);
	}

	public class UnitySpawner<T> where T : Component, ISpawnable {
		[SerializeField] private T prefabDefault;

		private List<T> activeList = new List<T>();
		private List<T> inactiveList = new List<T>();

		public UnitySpawner() {
		}

		public UnitySpawner(T prefab) {
			this.prefabDefault = prefab;
		}

		public T Spawn() {
			return this.Spawn(this.prefabDefault, Vector3.zero, Quaternion.identity);
		}

		public T Spawn(T prefab) {
			return this.Spawn(prefab, Vector3.zero, Quaternion.identity);
		}

		public T Spawn(Vector3 position) {
			return this.Spawn(this.prefabDefault, position, Quaternion.identity);
		}

		public T Spawn(Transform transform) {
			return this.Spawn(this.prefabDefault, transform.position, transform.rotation);
		}

		public T Spawn(Vector3 position, Quaternion rotation) {
			return this.Spawn(this.prefabDefault, position, rotation);
		}

		public T Spawn(T prefab, Vector3 position) {
			return this.Spawn(prefab, position, Quaternion.identity);
		}

		public T Spawn(T prefab, Transform transform) {
			return this.Spawn(prefab, transform.position, transform.rotation);
		}

		public T Spawn(T prefab, Vector3 position, Quaternion rotation) {
			T spawnling;

			if (inactiveList.Count > 0) {
				spawnling = inactiveList[inactiveList.Count - 1];
				inactiveList.RemoveAt(inactiveList.Count - 1);
				spawnling.transform.position = position;
				spawnling.transform.rotation = rotation;
			}
			else {
				spawnling = GameObject.Instantiate(prefab, position, rotation);
			}

			spawnling.gameObject.SetActive(true);
			activeList.Add(spawnling);
			spawnling.OnSpawn(this);
			return spawnling;
		}

		public void Despawn(T spawnling) {
			activeList.Remove(spawnling);
			inactiveList.Add(spawnling);
			spawnling.OnDespawn(this);
			spawnling.gameObject.SetActive(false);
		}
	}

	public class CodeSpawner<T> where T : ISpawnable, new() {
		private List<T> activeList = new List<T>();
		private List<T> inactiveList = new List<T>();

		public T Spawn() {
			T spawnling;

			if (inactiveList.Count > 0) {
				spawnling = inactiveList[inactiveList.Count - 1];
				inactiveList.RemoveAt(inactiveList.Count - 1);
			}
			else {
				spawnling = new T();
			}

			activeList.Add(spawnling);
			spawnling.OnSpawn(this);
			return spawnling;
		}

		public void Despawn(T spawnling) {
			activeList.Remove(spawnling);
			inactiveList.Add(spawnling);
			spawnling.OnDespawn(this);
		}
	}
}
