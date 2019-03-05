using UnityEngine;
using System.Collections;

namespace Completed
{
	//怪物继承自移动物体，是所有可移动对象的基类，玩家也继承于此。
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Enemy : MovingObject
	{
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.
		
		//僵尸的动画组件
		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		//存储目标的变换
		private Transform target;							//Transform to attempt to move toward each turn.
		//僵尸是否跳过一回合的布尔值
		private bool skipMove;								//Boolean to determine whether or not enemy should skip a turn or move this turn.
		
		//重写基类的start函数，加一下子类需要单独处理的事情
		//Start overrides the virtual Start function of the base class.
		protected override void Start ()
		{
			Debug.Log("Enemy Start()");
			//将僵尸加入GameManager的敌人列表中，使其可以调用僵尸的移动指令。
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddEnemyToList (this);
			
			//获取动画组件。
			//Get and store a reference to the attached Animator component.
			animator = GetComponent<Animator> ();
			
			//找到标签为Player的GameObject的变换并存储为目标对象
			//Find the Player GameObject using it's tag and store a reference to its transform component.
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			
			//执行基类的通用方法
			//Call the start function of our base class MovingObject.
			base.Start ();
		}
		
		//重写基类的AttemptMove函数，加入是否跳过一回合的判断。
		//Override the AttemptMove function of to include functionality needed for Enemy to skip turns.
		//See comments in MovingObject for more on how base AttemptMove function works.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//判断一下是僵尸行动回合还是休息回合，休息回合则让下个回合变为行动回合。
			//Check if skipMove is true, if so set it to false and skip this turn.
			if(skipMove)
			{
				skipMove = false;
				return;
				
			}
			
			//调用基类的方法进行移动
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);
			
			//僵尸已经移动过了，等一回合让玩家喘口气
			//Now that Enemy has moved, set skipMove to true to skip next move.
			skipMove = true;
		}
		
		
		//该方法在GameManger中每个回合被调用，用于将怪物移向玩家。
		//MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
		public void MoveEnemy ()
		{
			//声明X和Y轴的移动方向，从-1到1，可以操纵上下左右的移动
			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			int xDir = 0;
			int yDir = 0;
			
			//判断X轴是否离目标玩家很近（判断依据float.Epsilon：小正数，一个极小的数字，但不是0）
			//If the difference in positions is approximately zero (Epsilon) do the following:
			if(Mathf.Abs (target.position.x - transform.position.x) < float.Epsilon)
				
			    //骚操作，三元操作符，进行Y轴距离的判断和Y轴方向赋值。
				//If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
				yDir = target.position.y > transform.position.y ? 1 : -1;
			
			//If the difference in positions is not approximately zero (Epsilon) do the following:
			else
				//如果X轴距离不接进，则优先进行X轴的判断和X轴方向赋值。
				//Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
				xDir = target.position.x > transform.position.x ? 1 : -1;
			
			//利用刚才判断并赋值完毕的X、Y轴方向，调用AttemptMove并传入Player作为碰撞对象进行移动。
			//Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
			AttemptMove <Player> (xDir, yDir);
		}
		
		//重写自基类函数，处理僵尸碰撞到玩家的情况，传入T作为泛型参数，此处应该为玩家player。
		//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
		protected override void OnCantMove <T> (T component)
		{
			//声明一个Player对象并赋值为碰撞到的玩家。
			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;
			
			//调用玩家的掉血函数。
			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			hitPlayer.LoseFood (playerDamage);
			
			//播放动画。
			//Set the attack trigger of animator to trigger Enemy attack animation.
			animator.SetTrigger ("enemyAttack");
			
			//调用声音管理内的随机播放音效。
			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}
	}
}
