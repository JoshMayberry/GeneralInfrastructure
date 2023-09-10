using UnityEngine;
using UnityEngine.SceneManagement;

namespace jmayberry.GeneralInfrastructure {
	public class LevelTransition : MonoBehaviour {
		Animator anim;
		string levelName;

		void Start() {
			this.anim = this.GetComponent<Animator>();
		}

		public void ChangeScene(string sceneName) {
			this.levelName = sceneName;
			this.anim.SetTrigger("fadeIn");
		}

		// Called by animation
		public void GoToScene() {
			SceneManager.LoadScene(this.levelName);
		}
	}
}
