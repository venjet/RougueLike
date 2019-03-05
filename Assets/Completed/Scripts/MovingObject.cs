using UnityEngine;
using System.Collections;

namespace Completed
{
	//关键字abstract抽象可以创建不完整的类和类成员，而子类中必须实现。
	//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
	public abstract class MovingObject : MonoBehaviour
	{
		//对象单次移动秒数
		public float moveTime = 0.1f;			//Time it will take object to move, in seconds.
		
		//申明一个阻挡层
		public LayerMask blockingLayer;			//Layer on which collision will be checked.
		
		//挂一个2D碰撞组件在本组件上
		private BoxCollider2D boxCollider; 		//The BoxCollider2D component attached to this object.
		//挂一个2D刚体组件在本组件上
		private Rigidbody2D rb2D;				//The Rigidbody2D component attached to this object.
		//除法转换乘法存储，用空间换时间
		private float inverseMoveTime;			//Used to make movement more efficient.
		
		
		//受保护的虚函数，可被子类重写。
		//Protected, virtual functions can be overridden by inheriting classes.
		protected virtual void Start ()
		{
			//获取本体的2D碰撞组件。
			//Get a component reference to this object's BoxCollider2D
			boxCollider = GetComponent <BoxCollider2D> ();
			
			//获取本体的2D刚体组件。
			//Get a component reference to this object's Rigidbody2D
			rb2D = GetComponent <Rigidbody2D> ();
			
			//先将移动时间倒数一下，之后再用的时候只需要做乘法而不是除法，执行效率更高。
			//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
			inverseMoveTime = 1f / moveTime;
		}
		
		
		//该函数通过X,Y轴方向和一个2D碰撞检测来返回是否可以移动的信息。
		//Move returns true if it is able to move and false if not. 
		//Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
		protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
		{
			//获取当前可移动物体的起始位置Vector2
			//Store start position to move from, based on objects current transform position.
			Vector2 start = transform.position;
			
			//叠加上X,Y轴的移动后，获取终点位置。
			// Calculate end position based on the direction parameters passed in when calling Move.
			Vector2 end = start + new Vector2 (xDir, yDir);
			
			//使自身碰撞失效，不然起点是自身中心，路径射线射出去就碰到了自己。详见>>http://www.voidcn.com/article/p-znkzatfd-db.html
			//Disable the boxCollider so that linecast doesn't hit this object's own collider.
			boxCollider.enabled = false;
			
			//在阻挡层上，返回从起点到终点间射线上遇到的碰撞体。（P.S 2D和3D的函数第三个参数含义不同）
			//Cast a line from start point to end point checking collision on blockingLayer.
			hit = Physics2D.Linecast (start, end, blockingLayer);
			
			//恢复自身碰撞
			//Re-enable boxCollider after linecast
			boxCollider.enabled = true;
			
			//如果没有阻挡
			//Check if anything was hit
			if(hit.transform == null)
			{
				//协程一个平滑移动
				//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
				StartCoroutine (SmoothMovement (end));
				
				//返回可移动
				//Return true to say that Move was successful
				return true;
			}
			
			//如果有阻挡，返回不可移动
			//If something was hit, return false, Move was unsuccesful.
			return false;
		}
		
		//协程，平滑移动至终点
		//Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
		protected IEnumerator SmoothMovement (Vector3 end)
		{
			//计算当前位置与终点间的距离，这里使用了sqrMagnitude是因为他比magnitude更节省成本。
			//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
			//Square magnitude is used instead of magnitude because it's computationally cheaper.
			float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			
			//距离是否已经足够近（老朋友Epsilon，接近0）
			//While that distance is greater than a very small amount (Epsilon, almost zero):
			while(sqrRemainingDistance > float.Epsilon)
			{
				//根据自身刚体的位置与终点的位置，除以移动时间，得出一小步的位置。
				//该方法会在uadate中被调用，而update的简直不固定，因此乘以Time.deltaTime（上一帧至今的时间）
				//Find a new position proportionally closer to the end, based on the moveTime
				Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
				
				//利用2D刚体自带的移动函数移动一小步。
				//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
				rb2D.MovePosition (newPostion);
				
				//获取现在还剩的距离。
				//Recalculate the remaining distance after moving.
				sqrRemainingDistance = (transform.position - end).sqrMagnitude;
				
				//歇一下，待会接着来
				//Return and loop until sqrRemainingDistance is close enough to zero to end the function
				yield return null;
			}
		}
		
		//虚函数，可被继承的类用overridden关键字重写。
		//该函数利用泛型参数T来传入一个组件，用于判断是否有碰撞。
		//The virtual keyword means AttemptMove can be overridden by inheriting classes using the override keyword.
		//AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact with if blocked (Player for Enemies, Wall for Player).
		protected virtual void AttemptMove <T> (int xDir, int yDir)
			//使用where关键字约束泛型T必须是组件类型
			where T : Component
		{
			//声明一个hit信息
			//Hit will store whatever our linecast hits when Move is called.
			RaycastHit2D hit;
			
			//通过Move函数给canMove赋值T or F，真值的话，完成平滑移动的过程。
			//Set canMove to true if Move was successful, false if failed.
			bool canMove = Move (xDir, yDir, out hit);
			
			//判断canMove中射线碰撞体是否存在
			//Check if nothing was hit by linecast
			if(hit.transform == null)
				//如果没有碰撞，后续代码无需执行，跳出该函数。
				//If nothing was hit, return and don't execute further code.
				return;
			
			//获取碰撞到的泛型参数的组件
			//Get a component reference to the component of type T attached to the object that was hit
			T hitComponent = hit.transform.GetComponent <T> ();
			
			//如果不能动而且确实碰到了一个同泛型参数的碰撞体，说明当前的组件可以和他发生点什么。
			//If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
			if(!canMove && hitComponent != null)
				
				//调用OnCanMove方法并将碰撞到的组件传入。
				//Call the OnCantMove function and pass it hitComponent as a parameter.
				OnCantMove (hitComponent);
		}
		
		//抽象方法，等待基类来实现。
		//The abstract modifier indicates that the thing being modified has a missing or incomplete implementation.
		//OnCantMove will be overriden by functions in the inheriting classes.
		protected abstract void OnCantMove <T> (T component)
			where T : Component;
	}
}
