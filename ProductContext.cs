namespace Hillinworks.WorkflowFramework
{
    public sealed class ProductContext<T> : ProductContext
    {
        public T Product => (T)base.InternalProduct;

        internal ProductContext(ProductContext parent, T product)
            : base(parent, product)
        {

        }

        public ProductContext<TProduct> CreateChild<TProduct>(TProduct product)
        {
            return new ProductContext<TProduct>(this, product);
        }

        public ProductContext<TProduct> CreateSibling<TProduct>(TProduct product)
        {
            return new ProductContext<TProduct>(this.Parent, product);
        }

    }

    public abstract class ProductContext
    {
        public static ProductContext<T> Create<T>(T product)
        {
            return new ProductContext<T>(null, product);
        }

        internal ProductContext Parent { get; }
        internal object InternalProduct { get; }

        internal ProductContext(ProductContext parent, object product)
        {
            this.Parent = parent;
            this.InternalProduct = product;
        }

        public ProductContext<TProduct> Resolve<TProduct>()
        {
            var context = this;
            while (context != null && !(context.InternalProduct is TProduct))
            {
                context = context.Parent;
            }

            return (ProductContext<TProduct>)context;
        }
    }
}