using UnityEngine;

namespace Game.BattleTable
{
    [ExecuteAlways]
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemColor : MonoBehaviour
    {
        public void updateColor()
        {
            var main = particle.main;
            var lifetime = particle.colorOverLifetime;
            var speed = particle.colorBySpeed;
            var startGradient = _startGradient;
            var lifetimeGradient = _lifetimeGradient;
            var speedGradient = _speedGradient;
            if (!lifetime.enabled && !speed.enabled)
            {
                startGradient = processMinMaxGradient(startGradient);
            }
            else
            {
                if (lifetime.enabled)
                {
                    lifetimeGradient = processMinMaxGradient(lifetimeGradient);
                    if (speed.enabled)
                    {
                        speedGradient = processMinMaxGradientRGB(speedGradient);
                    }
                }
                else if (speed.enabled)
                {
                    speedGradient = processMinMaxGradient(speedGradient);
                }
            }
            main.startColor = startGradient;
            lifetime.color = lifetimeGradient;
            speed.color = speedGradient;
        }
        private void OnEnable()
        {
            _startGradient = particle.main.startColor;
            _lifetimeGradient = particle.colorOverLifetime.color;
            _speedGradient = particle.colorBySpeed.color;
            updateColor();
        }
        private void OnDisable()
        {
            var main = particle.main;
            var lifetime = particle.colorOverLifetime;
            var speed = particle.colorBySpeed;
            main.startColor = _startGradient;
            lifetime.color = _lifetimeGradient;
            speed.color = _speedGradient;
        }
        private void Start()
        {
            updateColor();
        }
        private Color getGreyscaleColor(Color color)
        {
            var comp = Mathf.Max(color.r, color.g, color.b);
            color.r = comp;
            color.g = comp;
            color.b = comp;
            return color;
        }
        private Color processRGB(Color color)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            h += _hue / 360;
            h = h < 0 ? 1 + h: h;
            print(h * 360);
            s = Mathf.Clamp01(s + _saturation / 100);
            v = Mathf.Clamp01(v + _value / 100);
            return Color.HSVToRGB(h, s, v);
        }
        private float processAlpha(float alpha)
        {
            return Mathf.Clamp01(alpha + _alpha / 100);
        }
        private Color processColor(Color color)
        {
            color = processRGB(color);
            color.a = processAlpha(color.a);
            return color;
        }
        private Gradient processGradientRGB(Gradient gradient)
        {
            var newGradient = new Gradient();
            var keys = gradient.colorKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                var colorKey = keys[i];
                colorKey.color = processRGB(colorKey.color);
                keys[i] = colorKey;
            }
            newGradient.colorKeys = keys;
            newGradient.alphaKeys = gradient.alphaKeys;
            return newGradient;
        }
        private Gradient processGradient(Gradient gradient)
        {
            var newGradient = processGradientRGB(gradient);
            var keys = gradient.alphaKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                var alphaKey = keys[i];
                alphaKey.alpha = processAlpha(alphaKey.alpha);
                keys[i] = alphaKey;
            }
            newGradient.alphaKeys = keys;
            return newGradient;
        }
        private ParticleSystem.MinMaxGradient processMinMaxGradient(ParticleSystem.MinMaxGradient gradient)
        {
            var newGradient = new ParticleSystem.MinMaxGradient()
            {
                mode = gradient.mode
            };
            switch (newGradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    newGradient.color = processColor(gradient.color);
                    break;
                case ParticleSystemGradientMode.Gradient:
                case ParticleSystemGradientMode.RandomColor:
                    newGradient.gradient = processGradient(gradient.gradient);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    newGradient.colorMax = processColor(gradient.colorMax);
                    newGradient.colorMin = processColor(gradient.colorMin);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    newGradient.gradientMax = processGradient(gradient.gradientMax);
                    newGradient.gradientMin = processGradient(gradient.gradientMin);
                    break;
            }
            return newGradient;
        }
        private ParticleSystem.MinMaxGradient processMinMaxGradientRGB(ParticleSystem.MinMaxGradient gradient)
        {
            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    gradient.color = processRGB(gradient.color);
                    break;
                case ParticleSystemGradientMode.Gradient:
                case ParticleSystemGradientMode.RandomColor:
                    gradient.gradient = processGradientRGB(gradient.gradient);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    gradient.colorMax = processRGB(gradient.colorMax);
                    gradient.colorMin = processRGB(gradient.colorMin);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    gradient.gradientMax = processGradientRGB(gradient.gradientMax);
                    gradient.gradientMin = processGradientRGB(gradient.gradientMin);
                    break;
            }
            return gradient;
        }

        public ParticleSystem particle
        {
            get
            {
                if (!_particleSystem)
                {
                    _particleSystem = GetComponent<ParticleSystem>();
                }
                return _particleSystem;
            }
            set
            {
                _particleSystem = value;
            }
        }
        public float hue
        {
            get => _hue;
            set => _hue = value;
        }
        public float saturation
        {
            get => _saturation;
            set => _saturation = value;
        }
        public float value
        {
            get => _value;
            set => _value = value;
        }
        public float alpha
        {
            get => _alpha;
            set => _alpha = value;
        }
        ParticleSystem.MinMaxGradient _startGradient;
        ParticleSystem.MinMaxGradient _lifetimeGradient;
        ParticleSystem.MinMaxGradient _speedGradient;
        private ParticleSystem _particleSystem;
        [SerializeField]
        [Range(-180, 180)]
        float _hue = 0;
        [SerializeField]
        [Range(-100, 100)]
        float _saturation = 0;
        [SerializeField]
        [Range(-100, 100)]
        float _value = 0;
        [SerializeField]
        [Range(-100, 100)]
        float _alpha = 0;
    }
}
