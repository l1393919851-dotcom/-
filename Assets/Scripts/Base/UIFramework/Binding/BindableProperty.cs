using System;
using System.Collections.Generic;

namespace Base.UIFramework.Binding
{
    /// <summary>
    /// 可绑定属性。当值发生变化时自动通知所有订阅者。
    /// 适合实现 MVVM 中的"数据 → UI"自动同步。
    /// 
    /// 用法：
    ///   public BindableProperty<int> Gold = new BindableProperty<int>(0);
    ///   Gold.OnValueChanged += (oldV, newV) => goldText.text = newV.ToString();
    ///   Gold.Value = 100; // 自动触发回调
    /// </summary>
    public class BindableProperty<T>
    {
        private T _value;

        /// <summary>值变化回调（旧值，新值）。</summary>
        public event Action<T, T> OnValueChanged;

        /// <summary>
        /// 构造函数，可指定初始值。
        /// </summary>
        public BindableProperty(T initial = default)
        {
            _value = initial;
        }

        /// <summary>
        /// 取/设当前值。设置时如果新值与旧值不等，会触发 OnValueChanged。
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                var old = _value;
                _value = value;
                OnValueChanged?.Invoke(old, value);
            }
        }

        /// <summary>
        /// 强制设置值并触发回调，即使新旧值相等（用于初始化时同步 UI）。
        /// </summary>
        public void ForceSet(T value)
        {
            var old = _value;
            _value = value;
            OnValueChanged?.Invoke(old, value);
        }

        /// <summary>
        /// 解除所有订阅。一般在面板销毁时调用。
        /// </summary>
        public void ClearListeners()
        {
            OnValueChanged = null;
        }

        /// <summary>
        /// 支持隐式转换：直接把 BindableProperty 当成 T 使用。
        /// </summary>
        public static implicit operator T(BindableProperty<T> p) => p._value;

        /// <summary>
        /// 转字符串：直接转底层值。
        /// </summary>
        public override string ToString() => _value?.ToString();
    }
}
