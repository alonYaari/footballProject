using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// important
using System.Collections.Generic;
using Sprites;
using Models;
using animation;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace runningMan2
{
    public enum stateAction {Neutral, Offense, Defense};
    public delegate void BallOwnerDelegate(Player owner);
    public class Game1 : Game
    {
        public BallOwnerDelegate informBallOwner;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D backgroundTexture;
        public static List<Sprite> sprites;
        public static List<FieldPlayer> fieldPlayers = new List<FieldPlayer>();
        public static int teamAScore = 0;
        public static int teamBScore = 0;
        int gameMinutesSinceStart;
        float elapsedTime = 0;

        //private Texture2D _texture;
        //private Texture2D _ball_texture;
        //private Vector2 _position;
        //private Vector2 _ball_position;
        private FieldPlayer p;
       
        //private Ball ball;
        private GoalKeeper goalKeeper;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            bool debug = true;
            int addition = debug ? 1000 : 0;
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            backgroundTexture = Content.Load<Texture2D>("soccerField");
            var ballAnimation = new Dictionary<string, Animation>()
            {
                // problem - frame count makes image sliced
                {"smol_ball1", new Animation(Content.Load<Texture2D>("ball/smol_ball"),1 ) },
                {"smol_ball2", new Animation(Content.Load<Texture2D>("ball/smol_ball2"),1 ) },
                {"smol_ball3", new Animation(Content.Load<Texture2D>("ball/smol_ball3"),1 ) },
                {"smol_ball4", new Animation(Content.Load<Texture2D>("ball/smol_ball4"),1 ) },
            };

            var animations = new Dictionary<string, Animation>()
            {
                {"WalkRight", new Animation(Content.Load<Texture2D>("player/WalkRight"),3 ) },
                {"WalkUp", new Animation(Content.Load<Texture2D>("player/WalkUp"),3 ) },
                {"WalkDown", new Animation(Content.Load<Texture2D>("player/WalkDown"),3 ) },
                {"WalkLeft", new Animation(Content.Load<Texture2D>("player/WalkLeft"),3 ) },
            };
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            sprites = new List<Sprite>()
            {

                // First team.
                new GoalKeeper(animations, 1,Logic.leftGoaliePos, "WalkRight")
                {position = Logic.leftGoaliePos, input = new Input(){up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D,} },
                new FieldPlayer(animations,10, 1, Logic.playerAStartingPoint + new Vector2(0,0), "WalkRight",Logic.rightGoalCenter)
                { position = Logic.playerAStartingPoint +new Vector2(0,0),input = new Input() { up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D, },},
                new FieldPlayer(animations,20, 1, Logic.playerAStartingPoint + new Vector2(50+addition,100), "WalkRight", Logic.rightGoalCenter)
                { position = Logic.playerAStartingPoint + new Vector2(50+addition,100), input = new Input() { up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D, },},
                new FieldPlayer(animations,30, 1, Logic.playerAStartingPoint - new Vector2(-50+addition,100), "WalkRight", Logic.rightGoalCenter)
                { position = Logic.playerAStartingPoint - new Vector2(-50+addition,100),input = new Input() { up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D, },},

                // Second Team
                new GoalKeeper(animations, 2,Logic.rightGoaliePos, "WalkLeft")
                {position = Logic.rightGoaliePos, input = new Input(){up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D,} },
                new FieldPlayer(animations,40, 2, Logic.playerBStartingPoint, "WalkLeft", Logic.leftGoalCenter)
                { position = Logic.playerBStartingPoint,input = new Input() { up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D, },},
                new FieldPlayer(animations,50, 2, Logic.playerBStartingPoint + new Vector2(2000,100), "WalkLeft", Logic.leftGoalCenter)
                { position = Logic.playerBStartingPoint + new Vector2(2000, 100), input = new Input() { up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D, },},
                new FieldPlayer(animations,60, 2, Logic.playerBStartingPoint - new Vector2(250,100), "WalkLeft", Logic.leftGoalCenter)
                { position = Logic.playerBStartingPoint - new Vector2(250,100),input = new Input() { up = Keys.W, down = Keys.S, left = Keys.A, right = Keys.D, },},
                new Ball(ballAnimation, Logic.centerOfPitch) {position=Logic.centerOfPitch} ,
            };
            // sprite[0-3] - team1
            // sprite[4-7] - team2
            // sprite[8] - BALL
            setRandomSpeed();
            p = (FieldPlayer)sprites[1];
            goalKeeper = (GoalKeeper)sprites[0];
            Globals.ball = (Ball)sprites[8];

            foreach (var fieldPlayer in sprites.Where(fieldPlayer => fieldPlayer is FieldPlayer)){fieldPlayers.Add((FieldPlayer)fieldPlayer);}

            //goalKeeper.gkCaught = new InformGkState(p.setCantChase);
            //goalKeeper.gkCaught = new InformGkState(goalKeeper.setHasBall);
            //goalKeeper.gkCaught += gkThrowBall;
            //goalKeeper.gkThrew = new InformGkState(removeOwnerShips);
            //goalKeeper.gkThrew += removeOwnerShips;
            //informBallOwner = new BallOwnerDelegate(updateOwnerShips);
        }
        protected override void Update(GameTime gameTime)
        {
            determineState();
            // Globals.ball
            foreach (var sprite in sprites)
            {
                if (sprite is FieldPlayer) {  ((FieldPlayer)sprite).Update(gameTime, sprites); }
                else if (sprite is GoalKeeper) { ((GoalKeeper)sprite).Update(gameTime, sprites); }
                else { ((Ball)sprite).Update(gameTime); }
                //sprite.Update(gameTime, sprites); 
            }
            
            base.Update(gameTime);
        }
        public void determineState() 
        {
            // what if gk has ball - for 1 frame
            if (Globals.ball.owner == null) { neutralize();}
            else 
            {
                switch (Globals.ball.owner.team) 
                {
                    case (1):
                        for (int playerNum = 1; playerNum < 4; playerNum++)
                        {
                            ((FieldPlayer)sprites[playerNum]).state = stateAction.Offense;
                            ((FieldPlayer)sprites[playerNum + 4]).state = stateAction.Defense;
                        }
                        break;
                    case (2):
                        for (int playerNum = 5; playerNum < 8; playerNum++)
                        {
                            ((FieldPlayer)sprites[playerNum]).state = stateAction.Offense;
                            ((FieldPlayer)sprites[playerNum - 4]).state = stateAction.Defense;
                        }
                        break;
                }
            }
        }
        public void neutralize() 
        {
            foreach (var sprite in sprites)
            {
                if (sprite is not Ball && sprite is not GoalKeeper) 
                {
                    ((FieldPlayer)sprite).state = stateAction.Neutral;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedTime >= 2f) // Change this to the desired interval
            {
                gameMinutesSinceStart++;
                elapsedTime = 0f; // Reset the elapsed time counter
                // Do any other actions that should happen every minute here

            }
            if (gameMinutesSinceStart == 45) 
            {
                halfTime();
            }

          


            var backgroundRect = new Rectangle(0, 0, (int)(backgroundTexture.Width * 1.1f), backgroundTexture.Height);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the background texture to fill the entire window
            _spriteBatch.Begin();
            _spriteBatch.Draw(backgroundTexture, GraphicsDevice.Viewport.Bounds, Color.White);

            SpriteFont scoreFont = Content.Load<SpriteFont>("Arial");

            // Draw team A name and goals
            _spriteBatch.DrawString(scoreFont, "Team A :", new Vector2(240, 5), Color.White);
            _spriteBatch.DrawString(scoreFont, teamAScore.ToString(), new Vector2(305, 5), Color.White);

            // Draw team B name and goals
            _spriteBatch.DrawString(scoreFont, "Team B :", new Vector2(470, 5), Color.White);
            _spriteBatch.DrawString(scoreFont, teamBScore.ToString(), new Vector2(535, 5), Color.White);

            // Draw remaining time
            _spriteBatch.DrawString(scoreFont, gameMinutesSinceStart.ToString(), new Vector2(390, 5), Color.White);


            foreach (var sprite in sprites)
            {
                sprite.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
        public void gkThrowBall() 
        {
            goalKeeper.throwBall(Globals.ball);
        }
       
        public void checkIfGoal() 
        {
            int teamNum = Logic.isInGoal(Globals.ball);
            if (teamNum != 0 && Globals.ball.returnToCenter)
            {
                Globals.ball.returnToCenter = false;
                // add score for matching goal
                var t = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    informBallOwner?.Invoke(new Player(null)); // sets everyone's hasball to false
                    Logic.afterGoalPositions(sprites);
                    ScoreBoard.addScoreByTeam(teamNum);
                });
            }
        }
        public void updateOwnerShips(Player owner) 
        {
            owner.hasBall = true;
            foreach (Sprite p in sprites)
            {
                if (p is not Ball && p != owner)
                {
                    //(Player)Pl = 
                       ((Player)p).hasBall = false;         
                }
            }
        }
        public void removeOwnerShips()
        {
            foreach (Sprite p in sprites)
            {
                if (p is not Ball)
                {
                    ((Player)p).hasBall = false;
                }
            }
        }
        public void setRandomSpeed() 
        {
            Random random = new Random();
            double doub = 0.2;
            foreach (var sprite in sprites) 
            {
                if (sprite is not Ball)
                    sprite.speed = 0.5f;
                        //(float)(random.Next(2,  8)*doub);
            }
        }
        
        public void halfTime() 
        {
            // Maybe added time.
            
            foreach(var fieldPlayer in fieldPlayers)
            {
                if (fieldPlayer.goalCenter.X > 200){fieldPlayer.goalCenter = Logic.leftGoalCenter;}
                else { fieldPlayer.goalCenter = Logic.rightGoalCenter; }
            }

            Logic.afterGoalPositions(sprites);
        }

    }
}
   

     
