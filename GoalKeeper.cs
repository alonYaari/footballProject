using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using runningMan2;
using Sprites;

namespace runningMan2
{
    public delegate void InformGkState();
    //public delegate int InformGkState(int val);

    public class GoalKeeper : Player
    {
        public GoalKeeper(Texture2D texture) : base(texture)
        {

        }
       

        public GoalKeeper(Dictionary<string, Animation> animations, int team, Vector2 startingPos, string animationName) 
            : base(animations, startingPos, animationName)
        {
            this.team = team;   
        }

       

        public override void Update(GameTime gt, List<Sprite> sprites)
        {
          

            base.Update(gt, sprites);
        }

        public void deflictOrCatch(Ball ball)
        {
            int limit = 4;
            // catch ball
            
            //gkCaught?.Invoke();

            if (ball.Velocity.X < limit )
            {
                ball.position = Vector2.Lerp(ball.position, position, 0.15f);
                hasBall = true;
                //gkCaught?.Invoke();
                
            }
            // deflict ball
            else
            {
                hasBall = false;
                Vector2 ballToGoalkeeper = Vector2.Normalize(this.position - ball.position);
                float angle = (float)Math.Atan2(ballToGoalkeeper.Y, ballToGoalkeeper.X);
                float angleDegrees = MathHelper.ToDegrees(angle);
                float deflectionAngle = Math.Abs(angleDegrees);

                // Update ball velocity to deflect in opposite direction
                Vector2 ballVelocityAfterDeflection = Vector2.Transform(ball.Velocity, Matrix.CreateRotationZ(MathHelper.ToRadians(deflectionAngle)));
                ball.Velocity = ballVelocityAfterDeflection;

                ball.position += ball.Velocity;
            }
        }
    
        public void throwBall(Ball b)
        {
           
            b.Velocity = Vector2.Zero;
            b.Velocity.X = -5;
            b.Velocity.Y = -1;
            hasBall = false;
        }

        //public void JumpTowardsBall(Ball ball)
        //{
        //    if (Vector2.Distance(ball.position, this._position) <= 200f)
        //    {
        //        float timeToReachBall = 2f * JumpSpeed / JumpGravity;
        //        float jumpHeight = MaxJumpHeight - 0.5f * JumpGravity * timeToReachBall * timeToReachBall;
        //        float distanceToBall = ball.position.X - this._position.X;
        //        float jumpDistance = distanceToBall / 2f;
        //        float jumpTime = jumpDistance / JumpSpeed;
        //        float jumpVelocityY = -JumpGravity * jumpTime + JumpGravity * timeToReachBall;
        //        _jumpVelocity = new Vector2(distanceToBall / timeToReachBall, jumpVelocityY);
        //        _isJumping = true;

                
        //    }
        //}

    }
}
