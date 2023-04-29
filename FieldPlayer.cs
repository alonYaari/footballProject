using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using animation;
using Sprites;
namespace runningMan2
{
    public enum attackingDecision{Shoot, Pass, Dribble }
    public class FieldPlayer : Player
    {
        public bool canChase = true;
        public int id;
        public Vector2 prevVelocity;
        public Vector2 goalCenter;
        public stateAction state;

        public FieldPlayer(Texture2D texture) : base(texture)
        {

        }
        public FieldPlayer(Dictionary<string, Animation> animations,int id, int team, Vector2 startingPos, string animationName, Vector2 goalCenter)
            : base(animations, startingPos,animationName)
        {
            this.id = id;
            this.team = team;
            this.goalCenter = goalCenter;
        }
       
        public override void Update(GameTime gt, List<Sprite> sprites)
        {
            Move();
            

            if (this.Velocity != Vector2.Zero)
            {

                SetAnimations();
                animationManager.Update(gt);
            }
            Vector2 prevPos = position;
            position += Velocity;
            if (Velocity != Vector2.Zero)
                prevVelocity = Velocity;
            Velocity = Vector2.Zero;
            if (!Logic.isInBounds(this)) { position = prevPos; }
            List<Sprite> evaluationSprites =  Sprite.copySprites(sprites);

          
            if (((Ball)sprites[8]).owner == this)
            {
                speed = team == 1 ? Math.Abs(speed) : -1*Math.Abs(speed);
                attackingDecision attackerDecision = getThisCopy(evaluationSprites, this.id).decideOffenseMove(evaluationSprites, 3);
                 

                switch (attackerDecision)
                {
                    case (attackingDecision.Shoot):
                        shootTheBall(Globals.ball);
                        break;
                    case (attackingDecision.Pass):
                        realPassTheBall(determinePassingPlayer(sprites), Globals.ball);
                        break;
                    case (attackingDecision.Dribble):
                        dribble();
                        break;
                }
            }
            else
            {
                int factor = team == 1 ? 1 : -1;
                switch (state)
                {
                    case (stateAction.Offense):
                        speed *= factor;                
                        moveOffenseNoBall(sprites);
                        break;
                    case (stateAction.Defense):
                        speed *= factor;
                        assignDefenseMove(sprites);
                        break;
                    case (stateAction.Neutral):
                        if (Globals.ball.position.X <= position.X)  
                        {
                            speed = Math.Abs(speed) * -1;}
                        else { speed = Math.Abs(speed); }
                        assignNeutralMove(sprites);
                        break;
                }
            }



            if (canTakeBall((Ball)sprites[8]))
            {

                this.hasBall = true;
                Globals.ball.owner = this;
                ((Ball)sprites[8]).owner = this;

            }
            base.Update(gt, sprites);
        }
        public void updatePosition()
        {
            Vector2 prevPos = position;
            position += Velocity;
            if (!Logic.isInBounds(this)) { position = prevPos; }
        }

        public FieldPlayer getThisCopy(List<Sprite> spritesCopy, int id) 
        {
            foreach(var sprite in spritesCopy.Where(sprite => sprite is FieldPlayer))
            {
                if (((FieldPlayer)sprite).id == id) 
                {
                    return (FieldPlayer)sprite;
                }
            }
            return null;
        }
        public double evaluate(attackingDecision decision, List<Sprite> spritesCopy, int moves, double successChance)
        {
            if (moves == 0)
            {
                return successChance;
            }
            else
            {
                switch (decision)
                {
                    case attackingDecision.Shoot:
                        // Odds for goal in case the player will shoot.
                        return (chanceOfScoring()) * successChance;
                    case attackingDecision.Pass:
                        FieldPlayer receiver = determinePassingPlayer(spritesCopy); // Decide where to pass the ball, return the receiver.
                        if (receiver == null) { return 0; }
                        else
                        { // reaching here means he can pass the ball(no interruptions)
                            double passRisk = calculatePassRisk(receiver);
                            passTheBall(receiver, (Ball)spritesCopy[8]);
                            attackingDecision afterPassAction = receiver.decideOffenseMove(spritesCopy, moves - 1); // Calculate what the best action is.
                            return receiver.evaluate(afterPassAction, spritesCopy, moves - 1, successChance * passRisk); // Will "make" the best action.
                        }   
                    case attackingDecision.Dribble:
                        dribble();
                        double driblleRisk = calculateDribbleRisk(spritesCopy); // Will calculate the risk in driblle and then driblle.
                        attackingDecision afterDriblleAction = decideOffenseMove(spritesCopy, moves - 1); // Will look for the next "right" move.
                        return evaluate(afterDriblleAction, spritesCopy, moves - 1, successChance*driblleRisk); // Make the next move.
                }
            }
            return 0; // Will never reach here.
        }

        public attackingDecision decideOffenseMove(List<Sprite> cpySprites, int moves)
        {
            List<Sprite> tmp = Sprite.copySprites(cpySprites);
            double shootOutcome = getThisCopy(cpySprites, this.id).evaluate(attackingDecision.Shoot, cpySprites, moves, 100);
            cpySprites = tmp;
            double passOutcome = getThisCopy(cpySprites, this.id).evaluate(attackingDecision.Pass, cpySprites, moves, 100);
            cpySprites = tmp;
            double dribbleOutcome = getThisCopy(cpySprites, this.id).evaluate(attackingDecision.Dribble, cpySprites, moves, 100);
            cpySprites = tmp;
            if (shootOutcome >= passOutcome)
            {
                if (shootOutcome <= dribbleOutcome)
                    return attackingDecision.Dribble;
                else
                    return attackingDecision.Shoot;
            }
            else
            {
                if (passOutcome >= dribbleOutcome)
                    return attackingDecision.Pass;
                else
                    return attackingDecision.Dribble;
            }
        }
        public void passTheBall(FieldPlayer receiver, Ball ball)
        {
            
            // maybe not needed
            this.hasBall = false;
            receiver.hasBall = true;

            ball.position = receiver.position; // add x and y
            ball.owner = receiver;
        }
        public double calculateDribbleRisk(List<Sprite> sprites) 
        {
            double dis = getDistance(findClosestOpposingPlayer(sprites));
            return getDistance(findClosestOpposingPlayer(sprites))/240;
        }
        public void dribble() 
        {
            Random side = new Random();
            this.Velocity.X = this.speed;
            switch (side.Next(0, 3))
            {
                
                case (0): // go straight
                    break;
                case (1): // go straight & down
                    this.Velocity.Y = this.speed;
                    break;
                case (2): // go straight & up
                    this.Velocity.Y = -this.speed;
                    break;
            }
            //this.speed *= 1.2f;
            this.updatePosition();

        }
        public double calculatePassRisk(FieldPlayer receiver) 
        {
            float distance = Vector2.Distance(position, receiver.position);
            return  1f - (distance / 500);
        }
        public double chanceOfScoring() {
            Vector2 goalPos = new Vector2(goalCenter.X+20, goalCenter.Y);
            float distanceFromGoal = Vector2.Distance(position, goalPos);
            // Calculate the angle (in radians) between the player's position and the goal's position
            float angleRadians = (float)Math.Atan2(goalPos.Y - position.Y, goalPos.X+20 - position.X);

            // Convert the angle from radians to degrees
            float angleDegrees = (float)(angleRadians * 180f / Math.PI);
            
            double scoreChance = ((Math.Abs(angleDegrees)) / 180f)  * (680-distanceFromGoal)/2300;
            if (distanceFromGoal < 70)
            {
                int x = 1;
            }
            return scoreChance;
        }

        public FieldPlayer determinePassingPlayer(List<Sprite> cpySprites)
        {
            double bestDistance = 1000;
            FieldPlayer bestPlayer = null;
            foreach(var sprite in cpySprites.Where(sprite => sprite is FieldPlayer))
            {
                FieldPlayer teamate = (FieldPlayer)sprite;
                if (this.isSameTeam(teamate))
                { 
                    // check on both defenders if one of them interceps + check distance of pass
                    foreach (var defenderSprite in cpySprites.Where(defenderSprite => defenderSprite is FieldPlayer && 
                        !this.isSameTeam((FieldPlayer)defenderSprite)))
                    {
                         //FieldPlayer defenderPlayer = (FieldPlayer)defenderSprite;
                        if (!WillIntercept(position, teamate.position,defenderSprite.position, 5f, 5f))
                        {
                           if (bestDistance > getDistance(teamate)) // getting closer to goal 
                           {
                                // this can cause fail if decided to pass even though every1 is behind
                               if (teamate.goalCenter.X > 200 && position.X < teamate.position.X) // Meaning right goal
                               {
                                    bestDistance = getDistance(teamate);
                                    bestPlayer = teamate;
                               }

                               else if(teamate.goalCenter.X < 200 && position.X > teamate.position.X)// Meaning left goal
                               {
                                    bestDistance = getDistance(teamate);
                                    bestPlayer = teamate;
                               } 
                           }
                        }
                    }
                }
            }
            return bestPlayer;
        }
        public bool WillIntercept(Vector2 source, Vector2 destination, Vector2 defender, float ballRadius, float playerRadius)
        {
            // cj code
            // ball and player (defender) radiuses tbd 
            Vector2 direction = destination - source;
            float distance = direction.Length();
            Vector2 unitVector = direction / distance;
            Vector2 relativePosition = defender - source;
            float dotProduct = Vector2.Dot(unitVector, relativePosition);

            if (dotProduct < 0 || dotProduct > distance)
            {
                return false;
            }

            Vector2 projection = source + unitVector * dotProduct;
            float distanceToProjection = Vector2.Distance(projection, defender);

            //
            return distanceToProjection < ballRadius + playerRadius;
        }
        public bool canTakeBall(Ball ball)
        {
            return Vector2.Distance(position, ball.position) < 23 && ball.owner == null;
        }
        public double getDistance(FieldPlayer fp) { return Vector2.Distance(position, fp.position); }
        public void assignDefenseMove(List<Sprite> sprites)
        {
            if (isClosestToBallInTeam(sprites)) { onBallDefender(); }
            else { offBallDefender(sprites); }
        }
        public void offBallDefender(List<Sprite> sprites)
        {
            FieldPlayer closestOpp = findClosestOpposingPlayer(sprites);
            chaseTarget((int)((this.position.X + closestOpp.position.X) / 2f), (int)((this.position.Y + closestOpp.position.Y) / 2f));
        }
        public FieldPlayer findClosestOpposingPlayer(List<Sprite> cpySprites)
        {
            float minDistance = float.MaxValue;
            FieldPlayer closest = null;
            foreach (var opponent in cpySprites.Where(opponent => opponent is FieldPlayer && !((FieldPlayer)opponent).isSameTeam(this)))
            {
                float currDistance = Vector2.Distance(this.position, opponent.position);
                if (currDistance < minDistance && currDistance != 0)
                {
                    minDistance = currDistance;
                    closest = (FieldPlayer)opponent;
                }
            }
            return closest;
        }
        public void onBallDefender()
        {
            if (Vector2.Distance(position, Globals.ball.position) < 10f)
            {
                tryToTackle(); // insert delay so he doesnt spam tackle
            }
            else
            {
                chaseTarget((int)Globals.ball.position.X, (int)Globals.ball.position.Y);
            }
        }
        public void tryToTackle()
        {
            Random rand = new Random();
          //  if (rand.Next(100) == 1) { shootTheBall(); }
        }
       
        public void moveOffenseNoBall(List<Sprite>sprites) 
        {
            // get closer to  goal while trying to be clear for receiving a ball(dont let defender stand between you and teamate with ball)
            Vector2 ballPosition  = ((Ball)sprites[8]).position;

            // Get the vector pointing towards the goal
            Vector2 goalVector = goalCenter - position;

            // Get the vector pointing towards the ball handler
            Vector2 ballPointingVector = ballPosition - position;

            // Calculate the distance to the ball handler
            float distanceToBall = ballPointingVector.Length();

            // Calculate the angle between the goal vector and the ball pointing vector
            float angle = (float)Math.Acos(Vector2.Dot(goalVector, ballPointingVector) / (goalVector.Length() * ballPointingVector.Length()));

            // If the angle is less than 90 degrees(means the ball is infront of me), move towards the goal

            if (angle < MathHelper.PiOver2)
            {
                // Move towards the goal
                chaseTarget((int)goalCenter.X, (int)goalCenter.Y);
            }
            else
            {
                // If the ball handler is too far away, move towards the ball handler
                if (distanceToBall > 120f)
                {
                    // Move towards the ball handler
                    chaseTarget((int)ballPosition.X, (int) ballPosition.Y);
                }
                else
                {
                    // Otherwise, move towards x of goal in the same y
                    Random random = new Random();
                    chaseTarget((int)goalCenter.X - random.Next(-10,10), (int)position.Y);
                }
            }
            }
        public void assignNeutralMove(List<Sprite> sprites)
        {
            // if is closest to ball, chase ball
            if (isClosestToBallInTeam(sprites)) { chaseTarget((int)sprites[8].position.X, (int)(sprites[8].position.Y)); }
            // move like offense with no ball
            else { moveOffenseNoBall(sprites); }
        }



        public bool isClosestToBallInTeam(List<Sprite> sprites)
        {
            float minDistance = Vector2.Distance(Globals.ball.position, position);
            foreach (var sprite in sprites.Where(sprite=> sprite is FieldPlayer && this.isSameTeam((FieldPlayer)sprite)))
            { 
                float currDistance = Vector2.Distance(Globals.ball.position, sprite.position);
                if (currDistance < minDistance) { return false; }
            }
            return true;
        }
        public bool isSameTeam(FieldPlayer fp)
        {
            return team == fp.team && this != fp;
        }
        public void tryToScore(Vector2 GoalPos, Ball ball)
        {
            if (canChase)
            {
                Random rnd = new Random();
                if (hasBall)
                {
                    chaseTarget((int)GoalPos.X, (int)GoalPos.Y);
                    int shouldShoot = rnd.Next(1, 70);
                    if (shouldShoot == 5)
                    {
                        //shootTheBall();
                    }
                }
            }
        }


        public void shootTheBall(Ball ball)
        {
            // lower limit of shot power - wont shoot and ball stil stays in range
            int shootPow = 4;// Needs to be random
            double RandomFactor = 1.1;
            hasBall = false;
            ball.owner = null;
            Random random = new Random();
            Vector2 Randomness = new Vector2((float)(random.NextDouble() * RandomFactor), (float)(random.NextDouble() * RandomFactor));
            Randomness.Y = random.Next() % 2 == 0 ? -Randomness.Y : Randomness.Y; // decides to shoot up or down
            Randomness.X = prevVelocity.X < 0 ? -Randomness.X : Randomness.X;
            //ball.Velocity = (p.prevVelocity+Randomness) * shootPow;
            ball.Velocity = prevVelocity * shootPow; // could be a problem
            //p.onShootCooldown = true;
        }
        public void realPassTheBall(FieldPlayer target, Ball ball) 
        {
            Random passingForce = new Random();

            // Calculate the direction to the target player
            Vector2 directionToTarget = Vector2.Normalize(target.position - position);

            // Calculate the velocity of the ball based on the player's current velocity
            Vector2 ballVelocity = directionToTarget * (float)passingForce.Next(1,3)  + Velocity;

            // Pass the ball to the target player
            ball.Velocity = ballVelocity;
            ball.owner = null;
            //ball.Kick(ballVelocity, directionToTarget);

               
            
        }
        public void setCanChase() { canChase = true; }
        public void setCantChase() { canChase = false; }
        public void chaseBall( Ball ball)
        {
            if (!hasBall && canChase)
            {
                chaseTarget( (int)ball.position.X, (int)ball.position.Y);
            }
        }
        public  void chaseTarget(int x, int y)
        {
            Vector2 target = new Vector2(x, y);
            if (Vector2.Distance(target, position) >= 15)
            {
                if (target.X > position.X)
                {
                    Velocity.X = speed;
                }
                else if (target.X < position.X)
                {
                    Velocity.X = -speed;
                }
                if (target.Y > position.Y)
                {
                    Velocity.Y = speed;
                }
                else if (target.Y < position.Y)
                {
                    Velocity.Y = -speed;
                }
            }
        }
        public override void SetAnimations()
        {
            if (Velocity.X > 0)
            {
                animationManager.Play(animations["WalkRight"], true);
            }
            else if (Velocity.X < 0)
            {
                animationManager.Play(animations["WalkLeft"], true);
            }
            else if (Velocity.Y > 0)
            {
                animationManager.Play(animations["WalkDown"], true);
            }
            else if (Velocity.Y < 0)
            {
                animationManager.Play(animations["WalkUp"], true);
            }
        }
        public override void Move()
        {
            string pressed = "";
            if (Keyboard.GetState().IsKeyDown(input.left)) { Velocity.X = -speed; pressed += "l"; }
            if (Keyboard.GetState().IsKeyDown(input.right)) { Velocity.X = speed; pressed += "r"; }
            if (Keyboard.GetState().IsKeyDown(input.up)) { Velocity.Y = -speed; pressed += "u"; }
            if (Keyboard.GetState().IsKeyDown(input.down)) { Velocity.Y = speed; pressed += "d"; }
            if (pressed.Length > 1) { Velocity.X /= 1.4f; Velocity.Y /= 1.4f; }
           // if (Keyboard.GetState().IsKeyDown(input.shoot){ }
        }
       
    }
}
