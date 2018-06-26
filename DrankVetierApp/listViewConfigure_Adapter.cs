using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace DrankVetierApp
{
    class ListViewConfigure_Adapter : BaseAdapter<Layer>
    {
        Context context;
        List<Layer> Layers;
        //public EventOnTextChanged TxtChanged = new EventOnTextChanged();
        public event EventHandler<Custom_TextChangedArgs> TxtChanged;
        //public EventHandler<TextChangedEventArgs> NameChanged;
        //public EventHandler<TextChangedEventArgs> SpanChanged;

        public ListViewConfigure_Adapter(Context context, List<Layer> Layers)
        {
            this.context = context;
            this.Layers = Layers;
        }

        public override Layer this[int position]
        {
            get
            {
                return Layers[position];
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            ListViewConfigure_AdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as ListViewConfigure_AdapterViewHolder;

            if (holder == null)
            {
                holder = new ListViewConfigure_AdapterViewHolder();
                //var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //view = inflater.Inflate(Resource.Layout.Options_Item, parent, false);
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Options_Item, parent, false);
                holder.Layer = view.FindViewById<TextView>(Resource.Id.textViewLayer);
                holder.Span = view.FindViewById<EditText>(Resource.Id.editTextSpan);
                holder.Name = view.FindViewById<EditText>(Resource.Id.editTextName);
                view.Tag = holder;
            }

            holder.Layer.Text = Convert.ToString(position + 1) + ".";
            holder.Span.Text = Convert.ToString(Layers[position].GetSpan());
            holder.Name.Text = Layers[position].GetName();

            //https://demonuts.com/android-listview-edittext/ https://www.google.com/search?q=edittext+get+position+in+listview+on+text+change+xamarin+android&client=firefox-b-ab&source=lnms&tbm=vid&sa=X&ved=0ahUKEwjkxbe_4O_bAhWJ6aQKHU3jBpwQ_AUICigB&biw=1920&bih=943 http://www.learn-android-easily.com/2013/06/using-textwatcher-in-android.html
            //holder.Span.TextChanged += SpanChanged;

            //holder.Name.TextChanged += NameChanged;
            //holder.Span.AddTextChangedListener(new TextWatcher(holder.Span.Text, position, "span"));
            //holder.Name.AddTextChangedListener(new TextWatcher(holder.Name.Text, position, "name"));
            holder.Span.TextChanged += (slender, e) =>
            {
                if (TxtChanged != null)
                    TxtChanged(null, new Custom_TextChangedArgs(holder.Span.Text, position, "span"));
            };
            holder.Name.TextChanged += (slender, e) =>
            {
                if (TxtChanged != null)
                    TxtChanged(null, new Custom_TextChangedArgs(holder.Name.Text, position, "name"));
            };

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return Layers.Count();
            }
        }
    }

    class ListViewConfigure_AdapterViewHolder : Java.Lang.Object
    {
        public TextView Layer { get; set; }
        public EditText Span { get; set; }
        public EditText Name { get; set; }
    }

}