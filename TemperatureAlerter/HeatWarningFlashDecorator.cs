using RGB.NET.Core;
using System;

namespace TemperatureAlerter
{
    class HeatWarningFlashDecorator : AbstractUpdateAwareDecorator, IBrushDecorator
    {
        private readonly Func<int> _getTemperatureCallback;
        private readonly int _warningTemperature;
        private readonly int _dangerTemerature;
        private readonly Color _warningColour;
        private readonly Color _dangerColour;
        private readonly double _flashIntervalInSeconds;
        private bool _on;
        private double _currentTimeDelta;

        public HeatWarningFlashDecorator(Func<int> getTemperature, int warningTemperature, int dangerTemperature, Color warningColour, Color dangerColour, double flashIntervalInSeconds = 1)
        {
            _getTemperatureCallback = getTemperature;
            _warningTemperature = warningTemperature;
            _dangerTemerature = dangerTemperature;
            _warningColour = warningColour;
            _dangerColour = dangerColour;
            _flashIntervalInSeconds = flashIntervalInSeconds / 2;
        }

        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color)
            =>
                _getTemperatureCallback() switch
                {
                    var t when _on && t >= _dangerTemerature => _dangerColour,
                    var t when _on && t >= _warningTemperature => _warningColour,
                    var t when t >= _warningTemperature => new Color(0, 0, 0),
                    _ => Color.Transparent
                };

        protected override void Update(double deltaTime)
        {
            _currentTimeDelta += deltaTime;

            if (_currentTimeDelta >= _flashIntervalInSeconds)
            {
                _currentTimeDelta = 0;
                _on = !_on;
            }
        }
    }
}
