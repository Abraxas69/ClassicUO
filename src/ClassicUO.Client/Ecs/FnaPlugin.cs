using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TinyEcs;

namespace ClassicUO.Ecs;

struct MouseContext
{
    public MouseState OldState, NewState;
}

readonly struct FnaPlugin : IPlugin
{
    public bool WindowResizable { get; init; }
    public bool MouseVisible { get; init; }
    public bool VSync { get; init; }


    public void Build(Scheduler scheduler)
    {
        scheduler.AddResource(new UoGame(MouseVisible, WindowResizable, VSync));
        scheduler.AddResource(Keyboard.GetState());
        scheduler.AddResource(new MouseContext());
        scheduler.AddEvent<KeyEvent>();
        scheduler.AddEvent<MouseEvent>();
        scheduler.AddEvent<WheelEvent>();

        scheduler.AddSystem(static (Res<UoGame> game, SchedulerState schedState) => {
            game.Value.BeforeLoop();
            game.Value.RunOneFrame();
            schedState.AddResource(game.Value.GraphicsDevice);
            game.Value.RunApplication = true;
        }, Stages.Startup);

        scheduler.AddSystem((Res<UoGame> game) => {
            game.Value.SuppressDraw();
            game.Value.Tick();

            FrameworkDispatcher.Update();
        }).RunIf((SchedulerState state) => state.ResourceExists<UoGame>());

        scheduler.AddSystem(() => Environment.Exit(0), Stages.AfterUpdate)
                    .RunIf(static (Res<UoGame> game) => !game.Value.RunApplication);

        scheduler.AddSystem(static (
            Res<GraphicsDevice> device,
            Res<Renderer.UltimaBatcher2D> batch,
            Res<GameContext> gameCtx,
            Res<MouseContext> mouseCtx,
            Query<Renderable, Not<TileStretched>> query,
            Query<(Renderable, TileStretched)> queryTiles
        ) => {
            device.Value.Clear(Color.AliceBlue);

            var center = Isometric.IsoToScreen(gameCtx.Value.CenterX, gameCtx.Value.CenterY, gameCtx.Value.CenterZ);
            center.X -= device.Value.PresentationParameters.BackBufferWidth / 2f;
            center.Y -= device.Value.PresentationParameters.BackBufferHeight / 2f;

            if (mouseCtx.Value.NewState.LeftButton == ButtonState.Pressed)
            {
                gameCtx.Value.CenterOffset.X += mouseCtx.Value.NewState.X - mouseCtx.Value.OldState.X;
                gameCtx.Value.CenterOffset.Y += mouseCtx.Value.NewState.Y - mouseCtx.Value.OldState.Y;
            }

            center -= gameCtx.Value.CenterOffset;

            var sb = batch.Value;
            sb.Begin();
            sb.SetBrightlight(1.7f);
            sb.SetStencil(DepthStencilState.Default);
            queryTiles.Each((ref Renderable renderable, ref TileStretched stretched) =>
                sb.DrawStretchedLand(
                    renderable.Texture,
                    renderable.Position - center,
                    renderable.UV,
                    ref stretched.Offset,
                    ref stretched.NormalTop,
                    ref stretched.NormalRight,
                    ref stretched.NormalLeft,
                    ref stretched.NormalBottom,
                    renderable.Color,
                    renderable.Z
                )
            );
            query.Each((ref Renderable renderable) =>
                sb.Draw
                (
                    renderable.Texture,
                    renderable.Position - center,
                    renderable.UV,
                    renderable.Color,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    renderable.Z
                )
            );
            sb.SetStencil(null);
            sb.End();
            device.Value.Present();
        }, Stages.AfterUpdate).RunIf((SchedulerState state) => state.ResourceExists<GraphicsDevice>());

        scheduler.AddSystem((EventWriter<KeyEvent> writer, Res<KeyboardState> oldState) => {
            var newState = Keyboard.GetState();

            foreach (var key in oldState.Value.GetPressedKeys())
                if (newState.IsKeyUp(key)) // [pressed] -> [released]
                    writer.Enqueue(new KeyEvent() { Action = 0, Key = key });

            foreach (var key in newState.GetPressedKeys())
                if (oldState.Value.IsKeyUp(key)) // [released] -> [pressed]
                    writer.Enqueue(new KeyEvent() { Action = 1, Key = key });
                else if (oldState.Value.IsKeyDown(key))
                    writer.Enqueue(new KeyEvent() { Action = 2, Key = key });

            oldState.Value = newState;
        }, Stages.AfterUpdate);

        scheduler.AddSystem((Res<MouseContext> mouseCtx) => mouseCtx.Value.NewState = Mouse.GetState(), Stages.BeforeUpdate);

        scheduler.AddSystem((EventWriter<MouseEvent> writer, EventWriter<WheelEvent> wheelWriter, Res<MouseContext> mouseCtx) => {
            if (mouseCtx.Value.NewState.LeftButton != mouseCtx.Value.OldState.LeftButton)
                writer.Enqueue(new MouseEvent() { Action = mouseCtx.Value.NewState.LeftButton, Button = Input.MouseButtonType.Left, X = mouseCtx.Value.NewState.X, Y = mouseCtx.Value.NewState.Y });
            if (mouseCtx.Value.NewState.RightButton != mouseCtx.Value.OldState.RightButton)
                writer.Enqueue(new MouseEvent() { Action = mouseCtx.Value.NewState.RightButton, Button = Input.MouseButtonType.Right, X = mouseCtx.Value.NewState.X, Y = mouseCtx.Value.NewState.Y });
            if (mouseCtx.Value.NewState.MiddleButton != mouseCtx.Value.OldState.MiddleButton)
                writer.Enqueue(new MouseEvent() { Action = mouseCtx.Value.NewState.MiddleButton, Button = Input.MouseButtonType.Middle, X = mouseCtx.Value.NewState.X, Y = mouseCtx.Value.NewState.Y });
            if (mouseCtx.Value.NewState.XButton1 != mouseCtx.Value.OldState.XButton1)
                writer.Enqueue(new MouseEvent() { Action = mouseCtx.Value.NewState.XButton1, Button = Input.MouseButtonType.XButton1, X = mouseCtx.Value.NewState.X, Y = mouseCtx.Value.NewState.Y });
            if (mouseCtx.Value.NewState.XButton2 != mouseCtx.Value.OldState.XButton2)
                writer.Enqueue(new MouseEvent() { Action = mouseCtx.Value.NewState.XButton2, Button = Input.MouseButtonType.XButton2, X = mouseCtx.Value.NewState.X, Y = mouseCtx.Value.NewState.Y });

            if (mouseCtx.Value.NewState.ScrollWheelValue != mouseCtx.Value.OldState.ScrollWheelValue)
                // FNA multiplies for 120 for some reason
                wheelWriter.Enqueue(new WheelEvent() { Value = (mouseCtx.Value.OldState.ScrollWheelValue - mouseCtx.Value.NewState.ScrollWheelValue) / 120 });

            mouseCtx.Value.OldState = mouseCtx.Value.NewState;
        }, Stages.AfterUpdate);

        scheduler.AddSystem((EventReader<KeyEvent> reader) => {
            foreach (var ev in reader.Read())
                Console.WriteLine("key {0} is {1}", ev.Key, ev.Action switch {
                    0 => "up",
                    1 => "down",
                    2 => "pressed",
                    _ => "unkown"
                });
        });

        scheduler.AddSystem((EventReader<MouseEvent> reader) => {
            foreach (var ev in reader.Read())
                Console.WriteLine("mouse button {0} is {1} at {2},{3}", ev.Button, ev.Action switch {
                    ButtonState.Pressed => "pressed",
                    ButtonState.Released => "released",
                    _ => "unknown"
                }, ev.X, ev.Y);
        }).RunIf((Res<UoGame> game) => game.Value.IsActive);

        scheduler.AddSystem((EventReader<WheelEvent> reader) => {
            foreach (var ev in reader.Read())
                Console.WriteLine("wheel value {0}", ev.Value);
        }).RunIf((Res<UoGame> game) => game.Value.IsActive);
    }

    struct KeyEvent
    {
        public byte Action;
        public Keys Key;
    }

    struct MouseEvent
    {
        public ButtonState Action;
        public Input.MouseButtonType Button;
        public int X, Y;
    }

    struct WheelEvent
    {
        public int Value;
    }

    sealed class UoGame : Microsoft.Xna.Framework.Game
    {
        public UoGame(bool mouseVisible, bool allowWindowResizing, bool vSync)
        {
            GraphicManager = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = vSync
            };
            IsFixedTimeStep = false;
            IsMouseVisible = mouseVisible;
            Window.AllowUserResizing = allowWindowResizing;
        }

        public GraphicsDeviceManager GraphicManager { get; }


        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // I don't want to update things here, but on ecs systems instead
        }

        protected override void Draw(GameTime gameTime)
        {
            // I don't want to render things here, but on ecs systems instead
        }
    }
}
