using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{
		//游戏核心参数初始化
		public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
		public float turnDelay = 0.1f;							//Delay between each Player turn.
		public int playerFoodPoints = 100;						//Starting value for Player food points.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.
		
		
		private Text levelText;									//Text to display current level number.
		private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
		private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		private int level = 1;									//Current level number, expressed in game as "Day 1".
		private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
		private bool enemiesMoving;								//Boolean to check if enemies are moving.
		private bool doingSetup = true;							//Boolean to check if we're setting up board, prevent Player from moving during setup.
		
		
		
		//Awake is always called before any Start functions
		void Awake()
		{
			Debug.Log("GameManager Awake()");
			//没看明白为啥要这个判断处理
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);	
			
			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);
			
			//敌人列表初始化
			//Assign enemies to a new List of Enemy objects.
			enemies = new List<Enemy>();
			
			//获取游戏场景管理组件（cs脚本）
			//Get a component reference to the attached BoardManager script
			boardScript = GetComponent<BoardManager>();
			
			//调用设置关卡的函数，此时level=1，布置第一层
			//Call the InitGame function to initialize the first level 
			InitGame();
		}

		
        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
		//这个方法调用一次，这个标签的参数指明了这个方法是在场景加载完后才会调用
		//（否则这个方法会在场景加载开始的时候就会调用，这不是我们想要的）
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
			Debug.Log("call CallbackInitialization()");
            //register the callback to be called everytime the scene is loaded
			// 使用+=操作符向事件注册监听函数，当事件被激活时监听函数便会被调用（该处为场景加载后，调用OnSceneLoaded）
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {	
			Debug.Log("call OnSceneLoaded()");
            instance.level++;
            instance.InitGame();
        }

		
		//Initializes the game for each level.
		//初始化每一层
		void InitGame()
		{
			//While doingSetup is true the player can't move, prevent player from moving while title card is up.
			//设施工状态为真，玩家无法移动
			doingSetup = true;
			
			//Get a reference to our image LevelImage by finding it by name.
			//通过名字找到loading图片
			levelImage = GameObject.Find("LevelImage");
			
			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
			//找到关卡文字，并获取其组件
			levelText = GameObject.Find("LevelText").GetComponent<Text>();
			
			//Set the text of levelText to the string "Day" and append the current level number.
			//设置该文字为“第x天”
			levelText.text = "Day " + level;
			
			//Set levelImage to active blocking player's view of the game board during setup.
			//将loading图片放置在最前面遮住后面的施工现场
			levelImage.SetActive(true);
			
			//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
			//委托HideLevelImage函数，两秒后执行。（撤掉loading图片，玩家可以行动）
			Invoke("HideLevelImage", levelStartDelay);
			
			//Clear any Enemy objects in our List to prepare for next level.
			//清除列表内所有敌人，为下一层做准备。
			enemies.Clear();
			
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			//调用场景管理脚本的函数来设置该层场景内容。
			boardScript.SetupScene(level);
			
		}
		
		
		//Hides black image used between levels
		//隐藏loading图片，设置玩家为可动。
		void HideLevelImage()
		{
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);
			
			//Set doingSetup to false allowing player to move again.
			doingSetup = false;
		}
		
		//Update is called every frame.
		//每帧都跑update
		void Update()
		{
			//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
			//玩家行动或者敌人移动中或者场景初始化时直接跳出
			if(playersTurn || enemiesMoving || doingSetup)
				
				//If any of these are true, return and do not start MoveEnemies.
				return;
			
			//Start moving enemies.
			//玩家不在行动，敌人也没移动，也没在初始化时，执行移动敌人的操作。
			//这边敌人移动用的是协程的骚操作，毕竟敌人的移动必须和玩家操作同时进行。
			StartCoroutine (MoveEnemies ());
		}
		
		//Call this to add the passed in Enemy to the List of Enemy objects.
		//enemy脚本start函数调用，用于enemy将自身的脚本挂在List里，方便之后操控enemy的移动
		public void AddEnemyToList(Enemy script)
		{
			//Add Enemy to List enemies.
			enemies.Add(script);
		}
		
		
		//GameOver is called when the player reaches 0 food points
		//游戏结束的相关操作，在player脚本中被调用
		public void GameOver()
		{
			//Set levelText to display number of levels passed and game over message
			//哀悼文字
			levelText.text = "After " + level + " days, you starved.";
			
			//Enable black background image gameObject.
			//用之前的幕布来遮一下
			levelImage.SetActive(true);
			
			//Disable this GameManager.
			//enabled居然也是关键字...理论上整个游戏都隐身了。也不来个restart啥的...
			enabled = false;
		}
		
		//Coroutine to move enemies in sequence.
		//协程：移动敌人
		IEnumerator MoveEnemies()
		{
			//While enemiesMoving is true player is unable to move.
			//通知update不用重复调用
			enemiesMoving = true;
			
			//Wait for turnDelay seconds, defaults to .1 (100 ms).
			//先礼后兵，返回去，这里先停个0.1秒
			yield return new WaitForSeconds(turnDelay);
			
			//If there are no enemies spawned (IE in first level):
			//没怪就再返回去，再停个0.1秒
			if (enemies.Count == 0) 
			{
				//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
				yield return new WaitForSeconds(turnDelay);
			}
			
			//Loop through List of Enemy objects.
			//逐个处理怪物移动
			for (int i = 0; i < enemies.Count; i++)
			{				
				
				//Call the MoveEnemy function of Enemy at index i in the enemies List.
				enemies[i].MoveEnemy ();
				
				//Wait for Enemy's moveTime before moving next Enemy, 
				yield return new WaitForSeconds(enemies[i].moveTime);
			}
			//Once Enemies are done moving, set playersTurn to true so player can move.
			//怪动完了，人能动了
			playersTurn = true;
			
			//Enemies are done moving, set enemiesMoving to false.
			//怪物行动完了
			enemiesMoving = false;
		}
	}
}

