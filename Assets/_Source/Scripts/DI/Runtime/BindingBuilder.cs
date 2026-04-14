public sealed class BindingBuilder
{
    private readonly Binding _binding;
    
    public BindingBuilder(Binding binding) => _binding = binding;
    
    public BindingBuilder AsSingle()
    {
        _binding.Lifetime = Lifetime.Singleton;
        return this;
    }

    public BindingBuilder AsTransient()
    {
        _binding.Lifetime = Lifetime.Transient;
        return this;
    }
}