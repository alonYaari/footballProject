using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
// important
using animation;
using Models;
using runningMan2;

namespace Sprites
{
    public class Sprite
    {
        public Vector2 defaultPos;
        protected AnimationManager animationManager;
        public Dictionary<string, Animation> animations;
        public Texture2D texture;
        public Vector2 _position;
        public Rectangle hitbox; // set when sprite updates


        public Vector2 position
        {
            get { return _position; }
            set
            {
                _position = value;
                if (animationManager != null) animationManager.position = _position;
            }
        }

        public Vector2 Velocity;
        public Color color = Color.White;
        public float speed;
        public Input input;
        public Rectangle rectangle 
        {
            get { return new Rectangle( (int)position.X, (int)position.Y, texture.Width, texture.Height); }
        }
        public Sprite() { }
        public Sprite(Texture2D texture) { this.texture = texture; }
        public Sprite(Dictionary<string, Animation> animations, Vector2 startingPosition, string animationName = null)
        {
            this.animations = animations;

            animationManager = animationName == null? new AnimationManager(animations.First().Value) : new AnimationManager(animations[animationName]);
            defaultPos = startingPosition;
        }
        public void clone(Sprite k)
        {
            this.input = k.input;
            this.Velocity = k.Velocity;
            this.texture = k.texture;
            this.speed = k.speed;
            this.defaultPos = k.defaultPos;
            this.position = k.position;
            this._position = k._position;
            this.hitbox = k.hitbox;
            //this.animationManager = k.animationManager;
            this.animations = k.animations;

        }
        public static List<Sprite> copySprites(List<Sprite> sprites) 
        {
            List<Sprite> copySprites = new List<Sprite>();
            foreach (var sprite in sprites)
            { 
                if (sprite is FieldPlayer) 
                {
                    FieldPlayer cp = new FieldPlayer(sprite.texture);
                    cp.clone(sprite);
                    cp.id = ((FieldPlayer)sprite).id;
                    cp.goalCenter = ((FieldPlayer)sprite).goalCenter;
                    cp.team = ((FieldPlayer)sprite).team;
                    copySprites.Add(cp);
                }
                else if (sprite is GoalKeeper)
                {
                    GoalKeeper cp = new GoalKeeper(sprite.texture);
                    cp.clone(sprite);
                    cp.team = ((GoalKeeper)sprite).team;
                    copySprites.Add(cp);
                }
                else
                {
                    Ball cp = new Ball(sprite.texture, sprite.position);
                    cp.clone(sprite);
                    copySprites.Add(cp);
                }
            }
            return copySprites;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, position, Color.White);
            }
            else if (animationManager != null) { animationManager.Draw(spriteBatch); }
            else { throw new Exception("something aint right"); }
        }
        public virtual void Update(GameTime gt, List<Sprite> sprites)
        {
            // do not change attribute texture!!!!!!!!!!!!!!
            Texture2D txt = animations.First().Value.Texture;
            hitbox = new Rectangle((int)_position.X, (int)_position.Y, 6,6);
        }
        public virtual void Update(GameTime gt) 
        {
            // ball is calling this method
            Texture2D txt = animations.First().Value.Texture;

            hitbox = new Rectangle((int)_position.X, (int)_position.Y, 4, 4);

        }



        private void Move()
        {
            if (Keyboard.GetState().IsKeyDown(input.left)) { Velocity.X = -speed; }
            if (Keyboard.GetState().IsKeyDown(input.right)) { Velocity.X = speed; }
            if (Keyboard.GetState().IsKeyDown(input.up)) { Velocity.Y = -speed; }
            if (Keyboard.GetState().IsKeyDown(input.down)) { Velocity.Y = speed; }
        }
        public bool isHitting(Sprite other) 
        {
            return hitbox.Intersects(other.hitbox);
        }

        #region collision
        protected bool isTouchingLeft(Sprite sprite)
        {
            return this.rectangle.Right + this.Velocity.X > sprite.rectangle.Left // is gonna slide and hit him
                && this.rectangle.Left < sprite.rectangle.Left  // is before him
                && this.rectangle.Bottom > sprite.rectangle.Top // how would bottom be above top and top below bottom?(y is lower when higher)
                && this.rectangle.Top < sprite.rectangle.Bottom;
        }
        protected bool isTouchingRight(Sprite sprite) 
        {
            return this.rectangle.Left + this.Velocity.X < sprite.rectangle.Right
                && this.rectangle.Right > sprite.rectangle.Right
                && this.rectangle.Bottom > sprite.rectangle.Top
                && this.rectangle.Top < sprite.rectangle.Bottom;

        }
        protected bool isTouchingTop(Sprite sprite)
        {
            return this.rectangle.Bottom + this.Velocity.Y > sprite.rectangle.Top
                && this.rectangle.Top < sprite.rectangle.Top
                && this.rectangle.Right > sprite.rectangle.Left
                && this.rectangle.Left < sprite.rectangle.Right;
        }
        protected bool isTouchingBottom(Sprite sprite)
        {
            return this.rectangle.Top + this.Velocity.Y < sprite.rectangle.Bottom
                && this.rectangle.Bottom > sprite.rectangle.Bottom
                && this.rectangle.Right > sprite.rectangle.Left
                && this.rectangle.Left < sprite.rectangle.Right;
        }

        #endregion

    }
}
