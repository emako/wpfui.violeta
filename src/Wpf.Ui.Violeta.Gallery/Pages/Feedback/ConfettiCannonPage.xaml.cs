using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Violeta.Controls;

namespace Wpf.Ui.Violeta.Gallery.Pages.Feedback;

public partial class ConfettiCannonPage
{
    private const string DemoToken = "ConfettiCannonDemo";

    public ConfettiCannonPage()
    {
        InitializeComponent();
    }

    private void ButtonBasicCannon_OnClick(object sender, RoutedEventArgs e)
    {
        ConfettiCannon.Fire(new ConfettiCannon.Options
        {
            ParticleCount = 100,
            Spread = 70,
            Origin = new Point(0.5, 0.6)
        });
    }

    private void ButtonRandomDirection_OnClick(object sender, RoutedEventArgs e)
    {
        var random = new Random();

        ConfettiCannon.Fire(new ConfettiCannon.Options
        {
            Angle = random.Next(55, 125),
            Spread = random.Next(50, 70),
            ParticleCount = random.Next(50, 100),
            Origin = new Point(0.5, 0.6)
        });
    }

    private void ButtonRealisticLook_OnClick(object sender, RoutedEventArgs e)
    {
        const int count = 200;

        Fire(0.25, new ConfettiCannon.Options { Spread = 26, StartVelocity = 55 });
        Fire(0.2, new ConfettiCannon.Options { Spread = 60 });
        Fire(0.35, new ConfettiCannon.Options { Spread = 100, Decay = 0.91, Scalar = 0.8 });
        Fire(0.1, new ConfettiCannon.Options { Spread = 120, StartVelocity = 25, Decay = 0.92, Scalar = 1.2 });
        Fire(0.1, new ConfettiCannon.Options { Spread = 120, StartVelocity = 45 });
        return;

        void Fire(double particleRatio, ConfettiCannon.Options options)
        {
            options.ParticleCount = (int)Math.Floor(count * particleRatio);
            ConfettiCannon.Fire(options);
        }
    }

    private void ButtonFireworks_OnClick(object sender, RoutedEventArgs e)
    {
        const int duration = 8 * 1000;

        var random = new Random();
        global::System.DateTime animationEnd = global::System.DateTime.Now + TimeSpan.FromMilliseconds(duration);

        var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };

        timer.Tick += OnTimerOnTick;
        timer.Start();
        return;

        void OnTimerOnTick(object? o, EventArgs _)
        {
            double timeLeft = (animationEnd - global::System.DateTime.Now).TotalMilliseconds;

            if (timeLeft <= 0)
            {
                timer.Tick -= OnTimerOnTick;
                timer.Stop();
            }

            var particleCount = (int)(50 * (timeLeft / duration));
            ConfettiCannon.Fire(new ConfettiCannon.Options
            {
                StartVelocity = 30,
                Spread = 360,
                Ticks = 60,
                ParticleCount = Math.Max(particleCount, 0),
                Origin = new Point
                {
                    X = RandomInRange(0.1, 0.3),
                    Y = random.NextDouble() - 0.2
                }
            });
            ConfettiCannon.Fire(new ConfettiCannon.Options
            {
                StartVelocity = 30,
                Spread = 360,
                Ticks = 60,
                ParticleCount = Math.Max(particleCount, 0),
                Origin = new Point
                {
                    X = RandomInRange(0.7, 0.9),
                    Y = random.NextDouble() - 0.2
                }
            });
        }
    }

    private void ButtonStars_OnClick(object sender, RoutedEventArgs e)
    {
        SetTimeout(0);
        SetTimeout(100);
        SetTimeout(200);
        return;

        void Shoot()
        {
            ConfettiCannon.Fire(new ConfettiCannon.Options
            {
                Spread = 360,
                Ticks = 50,
                Gravity = 0,
                Decay = 0.94,
                StartVelocity = 30,
                Colors = ["#FFE400", "#FFBD00", "#E89400", "#FFCA6C", "#FDFFB8"],
                ParticleCount = 40,
                Scalar = 1.2,
                Shapes = ["star"]
            });
            ConfettiCannon.Fire(new ConfettiCannon.Options
            {
                Spread = 360,
                Ticks = 50,
                Gravity = 0,
                Decay = 0.94,
                StartVelocity = 30,
                Colors = ["#FFE400", "#FFBD00", "#E89400", "#FFCA6C", "#FDFFB8"],
                ParticleCount = 10,
                Scalar = 0.75,
                Shapes = ["circle"]
            });
        }

        void SetTimeout(int delayMilliseconds)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delayMilliseconds) };
            timer.Tick += OnTimerOnTick;
            timer.Start();

            void OnTimerOnTick(object? o, EventArgs _)
            {
                Shoot();

                if (o is not DispatcherTimer t)
                {
                    return;
                }

                t.Tick -= OnTimerOnTick;
                t.Stop();
            }
        }
    }

    private void ButtonSnow_OnClick(object sender, RoutedEventArgs e)
    {
        const int duration = 8 * 1000;

        var random = new Random();
        global::System.DateTime animationEnd = global::System.DateTime.Now + TimeSpan.FromMilliseconds(duration);
        double skew = 1;

        new AnimationFrame().Start(_ =>
        {
            double timeLeft = (animationEnd - global::System.DateTime.Now).TotalMilliseconds;

            if (timeLeft <= 0)
            {
                return true;
            }

            int ticks = (int)Math.Max(200, 500 * (timeLeft / duration));
            skew = Math.Max(0.8, skew - 0.001);

            ConfettiCannon.Fire(new ConfettiCannon.Options
            {
                ParticleCount = 1,
                StartVelocity = 0,
                Ticks = ticks,
                Origin = new Point
                {
                    X = random.NextDouble(),
                    Y = random.NextDouble() * skew - 0.2
                },
                Colors = ["#ffffff"],
                Shapes = ["circle"],
                Gravity = RandomInRange(0.4, 0.6),
                Scalar = RandomInRange(0.4, 1),
                Drift = RandomInRange(-0.4, 0.4),
            });

            return false;
        });
    }

    private void ButtonSchoolPride_OnClick(object sender, RoutedEventArgs e)
    {
        const int duration = 8 * 1000;

        global::System.DateTime animationEnd = global::System.DateTime.Now + TimeSpan.FromMilliseconds(duration);

        new AnimationFrame().Start(_ =>
        {
            double timeLeft = (animationEnd - global::System.DateTime.Now).TotalMilliseconds;
            if (timeLeft <= 0)
            {
                return true;
            }

            ConfettiCannon.Fire(new ConfettiCannon.Options
            {
                ParticleCount = 2,
                Angle = 60,
                Spread = 55,
                Origin = new Point(0, 0.5),
                Colors = ["#bb0000", "#ffffff"],
            });
            ConfettiCannon.Fire(new ConfettiCannon.Options
            {
                ParticleCount = 2,
                Angle = 120,
                Spread = 55,
                Origin = new Point(1, 0.5),
                Colors = ["#bb0000", "#ffffff"],
            });

            return false;
        });
    }

    private void ButtonCustomCanvas_OnClick(object sender, RoutedEventArgs e)
    {
        ConfettiCannon.Fire(new ConfettiCannon.Options
        {
            Spread = 70,
            Origin = new Point(0.5, 1.2),
            Token = DemoToken
        });
    }

    private static double RandomInRange(double min, double max)
    {
        var random = new Random();
        return random.NextDouble() * (max - min) + min;
    }

    private sealed class AnimationFrame
    {
        private Func<double, bool>? _callback;
        private long _lastTicks;

        public void Start(Func<double, bool> callback)
        {
            _callback = callback;
            _lastTicks = global::System.DateTime.Now.Ticks;
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            long nowTicks = global::System.DateTime.Now.Ticks;
            double deltaTime = (nowTicks - _lastTicks) / 10000000.0;
            _lastTicks = nowTicks;

            if (_callback?.Invoke(deltaTime) == true)
            {
                CompositionTarget.Rendering -= OnRendering;
            }
        }
    }
}
