using System;
using Android.Text;
using Java.Lang;

namespace DrankVetierApp
{
    public class TextWatcher : Java.Lang.Object, ITextWatcher
    {
        private string text;
        private int position;
        private string type;

        public TextWatcher(string text, int position, string type)
        {
            this.text = text;
            this.position = position;
            this.type = type;
        }

        public TextWatcher()
        {
        }

        public void AfterTextChanged(IEditable s)
        {
            throw new NotImplementedException();
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            throw new NotImplementedException();
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            OnTxtChanged();
        }

        public void OnTxtChanged()
        {
            //if (TxtChanged != null)
            //    TxtChanged(null, new Custom_TextChangedArgs(text, position, type));
        }
        //public event EventHandler<Custom_TextChangedArgs> TxtChanged;

    }

    public class EventOnTextChanged
    {
        public event EventHandler<Custom_TextChangedArgs> TxtChanged;
    }

    public class Custom_TextChangedArgs
    {
        public string text;
        public int position;
        public string type;

        public Custom_TextChangedArgs(string text, int position, string type)
        {
            this.text = text;
            this.position = position;
            this.type = type;
        }
    }
}