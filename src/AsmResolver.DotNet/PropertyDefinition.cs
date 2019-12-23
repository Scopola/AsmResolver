using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Blob;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a single property in a type definition of a .NET module.
    /// </summary>
    public class PropertyDefinition : IMemberDescriptor, IOwnedCollectionElement<TypeDefinition>, IHasSemantics
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<TypeDefinition> _declaringType;
        private readonly LazyVariable<PropertySignature> _signature;
        private IList<MethodSemantics> _semantics;

        /// <summary>
        /// Initializes a new property definition.
        /// </summary>
        /// <param name="token">The token of the property.</param>
        protected PropertyDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _signature = new LazyVariable<PropertySignature>(GetSignature);
            _declaringType = new LazyVariable<TypeDefinition>(GetDeclaringType);
        }

        /// <summary>
        /// Creates a new property definition.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="signature">The signature of the property.</param>
        public PropertyDefinition(string name, PropertyAttributes attributes, PropertySignature signature)
            : this(new MetadataToken(TableIndex.Property,0))
        {
            Name = name;
            Attributes = attributes;
            Signature = signature;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the field.
        /// </summary>
        public PropertyAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the property uses a special name. 
        /// </summary>
        public bool IsSpecialName
        {
            get => (Attributes & PropertyAttributes.SpecialName) != 0;
            set => Attributes = (Attributes & ~PropertyAttributes.SpecialName)
                                | (value ? PropertyAttributes.SpecialName : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the property uses a special name used by the runtime.
        /// </summary>
        public bool IsRuntimeSpecialName
        {
            get => (Attributes & PropertyAttributes.RtSpecialName) != 0;
            set => Attributes = (Attributes & ~PropertyAttributes.RtSpecialName)
                                | (value ? PropertyAttributes.RtSpecialName : 0);
        }


        /// <summary>
        /// Gets or sets a value indicating the property has a default value.
        /// </summary>
        public bool HasDefault
        {
            get => (Attributes & PropertyAttributes.HasDefault) != 0;
            set => Attributes = (Attributes & ~PropertyAttributes.HasDefault)
                                | (value ? PropertyAttributes.HasDefault : 0);
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string FullName => FullNameGenerator.GetPropertyFullName(Name, DeclaringType, Signature);

        /// <summary>
        /// Gets or sets the signature of the property. This includes the property type, as well as any parameters the
        /// property might define.
        /// </summary>
        public PropertySignature Signature
        {
            get => _signature.Value;
            set => _signature.Value = value;
        }

        /// <inheritdoc />
        public ModuleDefinition Module => DeclaringType?.Module;

        /// <summary>
        /// Gets the type that defines the property.
        /// </summary>
        public TypeDefinition DeclaringType
        {
            get => _declaringType.Value;
            private set => _declaringType.Value = value;
        }

        ITypeDescriptor IMemberDescriptor.DeclaringType => DeclaringType;

        TypeDefinition IOwnedCollectionElement<TypeDefinition>.Owner
        {
            get => DeclaringType;
            set => DeclaringType = value;
        }

        /// <inheritdoc />
        public IList<MethodSemantics> Semantics
        {
            get
            {
                if (_semantics is null)
                    Interlocked.CompareExchange(ref _semantics, GetSemantics(), null);
                return _semantics;
            }
        }

        /// <summary>
        /// Obtains the name of the property definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;
        
        /// <summary>
        /// Obtains the signature of the property definition.
        /// </summary>
        /// <returns>The signature.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signature"/> property.
        /// </remarks>
        protected virtual PropertySignature GetSignature() => null;
        
        /// <summary>
        /// Obtains the declaring type of the property definition.
        /// </summary>
        /// <returns>The declaring type.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="DeclaringType"/> property.
        /// </remarks>
        protected virtual TypeDefinition GetDeclaringType() => null;

        /// <summary>
        /// Obtains the methods associated to this property definition.
        /// </summary>
        /// <returns>The method semantic objects.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Semantics"/> property.
        /// </remarks>
        protected virtual IList<MethodSemantics> GetSemantics() =>
            new OwnedCollection<IHasSemantics, MethodSemantics>(this);
        
        /// <inheritdoc />
        public override string ToString() => FullName;
    }
}