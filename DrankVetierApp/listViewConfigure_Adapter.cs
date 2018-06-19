using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DrankVetierApp
{
    class ListViewConfigure_Adapter : BaseAdapter<Layer>
    {

        Context context;
        List<Layer> Layers;

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
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.Options_Item, parent, false);
                holder.Layer = view.FindViewById<TextView>(Resource.Id.textViewLayer);
                holder.Span = view.FindViewById<EditText>(Resource.Id.editTextSpan);
                holder.Name = view.FindViewById<EditText>(Resource.Id.editTextName);
                view.Tag = holder;
            }

            holder.Layer.Text = Convert.ToString(position + 1) + ".";
            holder.Span.Text = Convert.ToString(Layers[position].GetSpan());
            holder.Name.Text = Layers[position].GetName();

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