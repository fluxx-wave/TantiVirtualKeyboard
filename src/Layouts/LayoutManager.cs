using System;
using System.Collections.Generic;
using System.Linq;

namespace HotVirtualKeyboard.Layouts
{
    public class LayoutManager
    {
        private static readonly Lazy<LayoutManager> _instance = new Lazy<LayoutManager>(CreateInstance);
        private static LayoutManager CreateInstance()
        {
            return new LayoutManager();
        }

        public static LayoutManager Instance
        {
            get { return _instance.Value; }
        }

        public event EventHandler<KeyboardLayout> CurrentLayoutChanged;

        private readonly List<KeyboardLayout> _availableLayouts;
        private KeyboardLayout _currentLayout;

        public IReadOnlyList<KeyboardLayout> AvailableLayouts
        {
            get { return _availableLayouts; }
        }

        public KeyboardLayout CurrentLayout
        {
            get { return _currentLayout; }
            set
            {
                if (_currentLayout != value && value != null)
                {
                    _currentLayout = value;
                    EventHandler<KeyboardLayout> handler = CurrentLayoutChanged;
                    if (handler != null)
                    {
                        handler(this, _currentLayout);
                    }
                }
            }
        }

        public LayoutManager()
        {
            _availableLayouts = new List<KeyboardLayout>
            {
                LayoutDefinitions.CreateTouchLayout(),
                LayoutDefinitions.CreateFullLayout(),
                LayoutDefinitions.CreateNumpadPinLayout(),
                LayoutDefinitions.CreateSymbolsLayout()
            };

            _currentLayout = _availableLayouts[0];
        }

        public void SelectLayoutById(string id)
        {
            var match = _availableLayouts.FirstOrDefault(l => string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                CurrentLayout = match;
            }
        }
    }
}
