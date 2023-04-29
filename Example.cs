using Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
namespace runningMan2
{
    public  class Example
    {
        public static List<Sprite> sprites;
        public static  GameTime gt;
        public static void InitExample(GameTime gt,List<Sprite> sprites)
        {
            Example.gt = gt;
            Example.sprites = new List<Sprite>();
            Example.sprites = sprites.ToList();
        }

        public static void demo() 
        {
            GoalKeeper t = new GoalKeeper(((GoalKeeper)sprites[1]).texture);
            t.clone((GoalKeeper)sprites[1]);
            t.position = Vector2.One;
            t.Update(gt, sprites);
        }
       
       

    }
}
