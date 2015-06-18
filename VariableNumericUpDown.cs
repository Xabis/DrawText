using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TriDelta.DrawTextMode {
    public class VariableNumericUpDown : NumericUpDown {
        decimal incshift = 0;
        decimal incctrl = 0;
        decimal incalt = 0;

        [Category("Data")]
        [Description("Increment to use when the Shift button is held")]
        public decimal IncrementShift {
            get { return incshift; }
            set { incshift = value; }
        }

        [Category("Data")]
        [Description("Increment to use when the Control button is held")]
        public decimal IncrementCtrl {
            get { return incctrl; }
            set { incctrl = value; }
        }

        [Category("Data")]
        [Description("Increment to use when the Alt button is held")]
        public decimal IncrementAlt {
            get { return incalt; }
            set { incalt = value; }
        }

        public override void UpButton() {
            decimal oldinc = this.Increment;

            if ((ModifierKeys & Keys.Shift) > 0)
                Increment = IncrementShift;
            else if ((ModifierKeys & Keys.Control) > 0)
                Increment = IncrementCtrl;
            else if ((ModifierKeys & Keys.Alt) > 0)
                Increment = IncrementAlt;

            if (Increment == 0)
                Increment = oldinc;

            base.UpButton();

            this.Increment = oldinc;
        }

        public override void DownButton() {
            decimal oldinc = this.Increment;

            if ((ModifierKeys & Keys.Shift) > 0)
                Increment = IncrementShift;
            else if ((ModifierKeys & Keys.Control) > 0)
                Increment = IncrementCtrl;
            else if ((ModifierKeys & Keys.Alt) > 0)
                Increment = IncrementAlt;

            if (Increment == 0)
                Increment = oldinc;

            base.DownButton();

            this.Increment = oldinc;
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            int notches = Math.Abs(e.Delta) / SystemInformation.MouseWheelScrollDelta;

            if (e.Delta > 0) {
                for (var i = 0; i < notches; i++)
                    this.UpButton();
            } else {
                for (var i = 0; i < notches; i++)
                    this.DownButton();
            }

            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
}
