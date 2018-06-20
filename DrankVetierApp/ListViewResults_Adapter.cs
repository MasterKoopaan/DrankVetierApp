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
    class ListViewResults_Adapter : BaseAdapter
    {

        Context context;
        List<Layer> layers;
        int[] amounts;

        public ListViewResults_Adapter(Context context, Rack rack)
        {
            this.context = context;
            layers = rack.layers;
            amounts = rack.amounts;
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
            ListViewResults_Adapter_NoDataViewHolder holder = null;

            if (view != null)
                holder = view.Tag as ListViewResults_Adapter_NoDataViewHolder;

            if (holder == null)
            {
                holder = new ListViewResults_Adapter_NoDataViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                view = inflater.Inflate(Resource.Layout.Main_item, parent, false);
                holder.Layer = view.FindViewById<TextView>(Resource.Id.textViewLayer);
                holder.Amount = view.FindViewById<TextView>(Resource.Id.textViewAmount);
                holder.Name = view.FindViewById<TextView>(Resource.Id.textViewName);
                view.Tag = holder;
            }

            holder.Layer.Text = Convert.ToString(position + 1) + ".";
            if (amounts != null)
            {
                holder.Amount.Text = Convert.ToString(amounts[position]);
            }
            holder.Name.Text = layers[position].GetName();

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return layers.Count();
            }
        }
    }

    class ListViewResults_Adapter_NoDataViewHolder : Java.Lang.Object
    {
        public TextView Layer { get; set; }
        public TextView Amount { get; set; }
        public TextView Name { get; set; }
    }
}