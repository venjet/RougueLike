using UnityEngine;
using System.Collections;

namespace Completed
{
	public class Wall : MonoBehaviour
	{
		public AudioClip chopSound1;				//1 of 2 audio clips that play when the wall is attacked by the player.
		public AudioClip chopSound2;				//2 of 2 audio clips that play when the wall is attacked by the player.
		public Sprite dmgSprite;					//Alternate sprite to display after Wall has been attacked by player.
		public int hp = 3;							//hit points for the wall.
		
		
		private SpriteRenderer spriteRenderer;		//Store a component reference to the attached SpriteRenderer.
		
		
		void Awake ()
		{
			Debug.Log("Wall Awake()");
			//获取一个精灵渲染器组件
			//Get a component reference to the SpriteRenderer.
			spriteRenderer = GetComponent<SpriteRenderer> ();
		}
		
		//被player类调用，用于处理玩家砸墙的行为。
		//DamageWall is called when the player attacks a wall.
		public void DamageWall (int loss)
		{
			//播放一个随机敲墙音效
			//Call the RandomizeSfx function of SoundManager to play one of two chop sounds.
			SoundManager.instance.RandomizeSfx (chopSound1, chopSound2);
			
			//将墙壁的精灵置成损坏的墙壁
			//Set spriteRenderer to the damaged wall sprite.
			spriteRenderer.sprite = dmgSprite;
			
			//墙壁的HP处理
			//Subtract loss from hit point total.
			hp -= loss;
			
			//墙壁的死亡处理
			//If hit points are less than or equal to zero:
			if(hp <= 0)
				//Disable the gameObject.
				gameObject.SetActive (false);
		}
	}
}
