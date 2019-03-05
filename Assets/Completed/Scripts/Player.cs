using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
		public int pointsPerFood = 10;				//Number of points to add to player food points when picking up a food object.
		public int pointsPerSoda = 20;				//Number of points to add to player food points when picking up a soda object.
		public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
		public Text foodText;						//UI Text to display current player food total.
		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
		public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
		public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
		public AudioClip gameOverSound;				//Audio clip to play when player dies.
		
		private Animator animator;					//Used to store a reference to the Player's animator component.
		private int food;                           //Used to store player food points total during level.
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		//Vector2.one是Vector2(1, 1)的缩略写法，这里用来标注一下触屏手机坐标原点。
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif
		
		//重写基类start方法
		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{
			Debug.Log("Player Start()");
			//获取动画组件
			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();
			
			//设置玩家血量，数值从GameManager中获取，因为需要过层传递。
			//Get the current food point total stored in GameManager.instance between levels.
			food = GameManager.instance.playerFoodPoints;
			
			//设置一下食物的文本显示。
			//Set the foodText to reflect the current player food total.
			foodText.text = "Food: " + food;
			
			//调用基类的start()函数。
			//Call the Start function of the MovingObject base class.
			base.Start ();
		}
		
		//置为失效时需要处理的事
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			//当物件失效时，在GameManager中存储当前血量，以便下一层再度激活玩家时读取。
			//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
			GameManager.instance.playerFoodPoints = food;
		}
		
		
		private void Update ()
		{
			//不是玩家回合时，直接跳出该函数。
			//If it's not the player's turn, exit the function.
			if(!GameManager.instance.playersTurn) return;
			
			//用于存储XY轴的移动方向
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.
			
			//各类设备的单独处理，首先是PC平台的unity客户端和网页插件。
			//Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER
			
			//通过input接口获取X轴方向。
			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//通过input接口获取Y轴方向。
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			//不存在斜向移动，因此优先X轴。
			//Check if moving horizontally, if so set vertical to zero.
			if(horizontal != 0)
			{
				vertical = 0;
			}
			
			//各类手机设备的情况。
			//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

			//是否有触摸的判断
			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
				//读入首个触摸点
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];
				
				//判断是否是刚按下去，如果是，则赋值位置作为原点坐标
				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch
					touchOrigin = myTouch.position;
				}
				
				//如果发现触摸结束，并且标记X轴的位置为正
				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//存入终点坐标
					//Set touchEnd to equal the position of this touch
					Vector2 touchEnd = myTouch.position;
					
					//获取X轴位移
					//Calculate the difference between the beginning and end of the touch on the x axis.
					float x = touchEnd.x - touchOrigin.x;
					
					//获取Y轴位移
					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;
					
					//将原点的X轴置为-1,用于防止再度进入else if的判断处理
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;
					
					//比较XY的大小，用于决定是水平移动还是垂直移动
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//骚操作三元组再度登场
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						horizontal = x > 0 ? 1 : -1;
					else						
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						vertical = y > 0 ? 1 : -1;
				}
			}
			
#endif //End of mobile platform dependendent compilation section started above with #elif

			//判断水平方向或垂直方向是否不为0
			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
				//调用AttemptMove函数进行移动处理，Wall作为对象。
				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				AttemptMove<Wall> (horizontal, vertical);
			}
		}
		
		//重写了父类的AttemptMove函数。
		//参数x,y给定移动的方向，泛型参数T调用时赋值为wall，作为阻挡？
		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//每次移动扣一点食物。
			//Every time player moves, subtract from food points total.
			food--;
			
			//更新界面上的食物数量文本。
			//Update food text display to reflect current score.
			foodText.text = "Food: " + food;
			
			//调用基类的AttempMove方法进行移动。传入的泛型参数wall用于处理撞墙。
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
			base.AttemptMove <T> (xDir, yDir);
			
			//定义一个碰撞用于记录移动中的碰撞对象。
			//Hit allows us to reference the result of the Linecast done in Move.
			RaycastHit2D hit;
			
			//判断是否能移动，用于播放主角的移动音效。
			//If Move returns true, meaning Player was able to move into an empty space.
			if (Move (xDir, yDir, out hit)) 
			{
				//播放一个移动的随机声音
				//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
			}
			
			//移动后减了食物，判断下挂了没
			//Since the player has moved and lost food points, check if the game has ended.
			CheckIfGameOver ();
			
			//设置玩家的回合结束。（轮到怪动了？）
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			GameManager.instance.playersTurn = false;
		}
		
		//重写了基类啥都没干的OnCantMove。
		//传入泛型参数wall，玩家撞墙后自动攻击并摧毁。
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//定义一个Wall对象并将它等于传入的墙对象。
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;
			
			//调用墙本身的方法用于造成1点伤害。
			//Call the DamageWall function of the Wall we are hitting.
			hitWall.DamageWall (wallDamage);
			
			//播放玩家攻击的动画
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");
		}
		
		//Collider2D组件自带函数，与其他Collider2D碰撞体碰撞时触发，撞到的东西通过参数other传入
		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
			//撞到出口
			//Check if the tag of the trigger collided with is Exit.
			if(other.tag == "Exit")
			{
				//1秒后，协程调用Restart函数
				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay);
				
				//将自身失活
				//Disable the player object since level is over.
				enabled = false;
			}
			
			//撞到了食物
			//Check if the tag of the trigger collided with is Food.
			else if(other.tag == "Food")
			{
				//加上食物的量
				//Add pointsPerFood to the players current food total.
				food += pointsPerFood;
				
				//更新界面上的食物量
				//Update foodText to represent current total and notify player that they gained points
				foodText.text = "+" + pointsPerFood + " Food: " + food;
				
				//随机播放一个吃食物的声音
				//Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
				SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
				
				//将食物失活
				//Disable the food object the player collided with.
				other.gameObject.SetActive (false);
			}
			
			//撞到了苏打（其实和食物逻辑一样，就是撞到的东西不一样而已，感觉有更合理的实现方法）
			//Check if the tag of the trigger collided with is Soda.
			else if(other.tag == "Soda")
			{
				//加上苏打的量
				//Add pointsPerSoda to players food points total
				food += pointsPerSoda;
				
				//更新界面上的食物量
				//Update foodText to represent current total and notify player that they gained points
				foodText.text = "+" + pointsPerSoda + " Food: " + food;
				
				//播放一个喝苏打的声音
				//Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
				SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
				
				//失苏打失活
				//Disable the soda object the player collided with.
				other.gameObject.SetActive (false);
			}
		}
		
		//重载场景时被调用，主要是进入下一层时被调用
		//Restart reloads the scene when called.
		private void Restart ()
		{
			//载入最后加载的场景，并设置为“唯一”模式，因此会替换掉当前的场景，且不需要加载所有场景物件。
			//因为重载了场景，因此会触发GameManager的AfterSceneLoad函数，用于重新生成下一层的内容
			//Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
		
		//在怪物类中被调用，用于处理怪撞到角色后的掉食物事件。
		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void LoseFood (int loss)
		{
			//播放玩家被打动画
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("playerHit");
			
			//扣食物
			//Subtract lost food points from the players total.
			food -= loss;
			
			//更新界面上的食物文本
			//Update the food display with the new total.
			foodText.text = "-"+ loss + " Food: " + food;
			
			//检查是否GameOver
			//Check to see if game has ended.
			CheckIfGameOver ();
		}
		
		//判断食物是否为0，如果是的话，调用结束游戏。
		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			//检查食物的情况。
			//Check if food point total is less than or equal to zero.
			if (food <= 0) 
			{
				//播放丧乐。
				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
				SoundManager.instance.PlaySingle (gameOverSound);
				
				//背景音乐停止。
				//Stop the background music.
				SoundManager.instance.musicSource.Stop();
				
				//调用GameManager的GameOver函数收拾残局。
				//Call the GameOver function of GameManager.
				GameManager.instance.GameOver ();
			}
		}
	}
}

