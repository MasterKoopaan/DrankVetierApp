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
        public event EventHandler<Custom_TextChangedArgs> TextChanged;

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

            //fill listitem with data
            holder.Layer.Text = Convert.ToString(position + 1) + ".";
            holder.Span.Text = Convert.ToString(Layers[position].GetSpan());
            holder.Name.Text = Layers[position].GetName();

            //add event liseners
            holder.Span.TextChanged += (slender, e) =>
            {
                if (TextChanged != null)
                    TextChanged(null, new Custom_TextChangedArgs(holder.Span.Text, position, "span"));
            };
            holder.Name.TextChanged += (slender, e) =>
            {
                TextChanged?.Invoke(null, new Custom_TextChangedArgs(holder.Name.Text, position, "name"));
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

    //holder with infladed listitem info 
    class ListViewConfigure_AdapterViewHolder : Java.Lang.Object
    {
        public TextView Layer { get; set; }
        public EditText Span { get; set; }
        public EditText Name { get; set; }
    }

    //custom event arguments class for TextChanged event
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