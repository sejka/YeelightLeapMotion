using Leap;
using System;
using System.Threading.Tasks;

namespace YeelightLeapMotion
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            var lightbulb = new YeelightLeapMotion("192.168.0.22");
            await lightbulb.Run();
        }
    }

    public class YeelightLeapMotion
    {
        private const int UpdateDelayMilliseconds = 500;
        private const int POWEROFF_TRESHOLD = 10;
        private static int _brightness = 1;
        private readonly string _bulbIp;

        public YeelightLeapMotion(string bulbIp)
        {
            _bulbIp = bulbIp;
        }

        public async Task Run()
        {
            var lightbulb = new YeelightAPI.Device(_bulbIp);

            await lightbulb.Connect();

            Controller controller = new Controller();
            controller.FrameReady += FrameReadyEvent;
            var oldBrightness = 0;
            var turnedOn = true;

            do
            {
                if (BrightnessDifferenceTresholdExceeded(oldBrightness))
                {
                    if (_brightness < POWEROFF_TRESHOLD)
                    {
                        await lightbulb.TurnOff(UpdateDelayMilliseconds);
                        turnedOn = false;
                    }
                    else
                    {
                        if (!turnedOn)
                        {
                            await lightbulb.SetPower(true);
                            turnedOn = true;
                        }
                        await lightbulb.SetBrightness(_brightness, UpdateDelayMilliseconds);
                        oldBrightness = _brightness;
                    }
                }
                await Task.Delay(UpdateDelayMilliseconds);
            }
            while (true);
        }

        private static bool BrightnessDifferenceTresholdExceeded(int oldBrightness)
        {
            return Math.Abs(_brightness - oldBrightness) > 10;
        }

        private static void FrameReadyEvent(object sender, FrameEventArgs e)
        {
            Frame frame = e.frame;
            foreach (Hand hand in frame.Hands)
            {
                _brightness = (int)(hand.GrabAngle / Math.PI * 100);

                Console.WriteLine(_brightness);
            }
        }
    }
}