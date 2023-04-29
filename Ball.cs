using animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using runningMan2;
using Sprites;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


public class Ball : Sprite
{
    public bool isMoving = false;
    public bool returnToCenter = true;
    public Player owner = null;
    public Ball(Texture2D texture, Vector2 position)
        : base(texture)
    {
        this.position = position;
        this.Velocity = Vector2.Zero;
    }
    public Ball(Texture2D texture, Vector2 position, Vector2 velocity)
        : base(texture)
    {
        this.position = position;
        this.Velocity = velocity;
    }
    public Ball(Dictionary<string, Animation> animations, Vector2 startingPos) : base(animations, startingPos) { }

    public void Update(GameTime gameTime)
    {
        // texture = animations.First().Value.Texture;
        //hitbox = new Rectangle((int)_position.X, (int)_position.Y, texture.Width, texture.Height);
        base.Update(gameTime);
        //SetAnimations(gameTime, p);
        //animationManager.Update(gameTime);
       
        // Update the ball's position based on its velocity
        if (Logic.isInBounds(this) || Logic.isInGoal(this) != 0)
        {
            position += Velocity;
        }
        else 
        {
            
        }
       
        if (owner != null) 
        {
            Vector2 ballPosAdjast = determineBallPosAccordingToPlayer((FieldPlayer)owner);
            position = Vector2.Lerp(position, ballPosAdjast, 0.23f);
        }
        // 35 is random value that determines "rolls of ball"
        // this with velocity value in "shootTheBall" makes up the speed
        Velocity -= Velocity / 35;
        // update global ball so that he is same as current ball.
        Globals.ball = this;

    }
    public Vector2 determineBallPosAccordingToPlayer(FieldPlayer p)
    {
        Vector2 res = Vector2.Zero;
        bool direction = false;
        if (p.prevVelocity.X < 0) { direction = true; res = new Vector2(-25, 13); }
        if (p.prevVelocity.X > 0) { direction = true; res = new Vector2(25, 13); }
        if (p.prevVelocity.Y < 0)
        {
            if (!direction) { res += new Vector2(2, -6); }
            else { res += new Vector2(0, -12); }
        }
        if (p.prevVelocity.Y > 0)
        {
            if (!direction) { res += new Vector2(-2, 3); }
            else { res += new Vector2(0, 18); }
        }

        return res == Vector2.Zero ? Globals.ball.position : p.position + res;
    }

    public void SetAnimations(GameTime gameTime, Vector2 Velocity)
    {
        System.Console.WriteLine(Velocity);
        if (Velocity.X > 0)
        {
            animationManager.Play(animations["smol_ball1"], true);
        }
        else if (Velocity.X < 0)
        {
            animationManager.Play(animations["smol_ball2"], true);
        }
        else if (Velocity.Y > 0)
        {
            animationManager.Play(animations["smol_ball3"], true);
        }
        else if (Velocity.Y < 0)
        {
            animationManager.Play(animations["smol_ball4"], true);
        }
        animationManager.Update(gameTime);
    }
}
