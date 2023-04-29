using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sprites;

namespace runningMan2
{
    public class Logic
    {
        public static Rectangle field = new Rectangle(48, 9, 690, 413);
        public static Vector2 centerOfPitch = new Vector2(390, 230);
        public static Vector2 playerAStartingPoint = new Vector2(160, 225);
        public static Vector2 playerBStartingPoint = new Vector2(620, 225);
        public static Rectangle rightGoal = new Rectangle(735, 187, 39, 83);
        public static Vector2 rightGoalCenter = new Vector2(rightGoal.Center.X, rightGoal.Center.Y);
        public static Rectangle leftGoal = new Rectangle(4, 185, 43, 83);
        public static Vector2 leftGoalCenter = new Vector2(leftGoal.Center.X, leftGoal.Center.Y);

        public static Vector2 rightGoaliePos = new Vector2(730, 228);
        public static Vector2 leftGoaliePos = new Vector2(52, 228);
        
        
        public static bool isInBounds(Sprite sp)
        {
            Rectangle spriteRect = new Rectangle((int)sp.position.X, (int)sp.position.Y, 4,4);
            return field.Intersects(spriteRect);
        }
        public static int isInGoal(Ball b) 
        {
            Rectangle ballRect = new Rectangle((int)b.position.X, (int)b.position.Y, 2, 2);
            return rightGoal.Intersects(ballRect) ? 1 : leftGoal.Intersects(ballRect) ? 2 : 0;
        }
        public static void afterGoalPositions(List<Sprite> sprites)
        {
            foreach (Sprite sp in sprites) 
            {
                sp.position = sp.defaultPos;
                sp.Velocity = Vector2.Zero;
                if (sp is Ball)
                    ((Ball)sp).returnToCenter = true;
            }
        }
        public static bool checkOutOfBounds(Ball ball) 
        {
            if (!field.Contains(ball.position) && !rightGoal.Contains(ball.position) && !leftGoal.Contains(ball.position))
            {
                ball.Velocity = Vector2.Zero;
                if (ball.position.X >= rightGoal.X)
                {
                    ball.position = rightGoaliePos + new Vector2(-10, -6);
                    return true;

                }
            }
            return false;

        }
    }
}
