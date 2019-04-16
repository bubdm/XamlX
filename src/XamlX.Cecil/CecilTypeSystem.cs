using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace XamlX.TypeSystem
{
    public partial class CecilTypeSystem : IXamlXTypeSystem,  IAssemblyResolver
    {
        private List<CecilAssembly> _asms = new List<CecilAssembly>();
        private Dictionary<string, CecilAssembly> _assemblyCache = new Dictionary<string, CecilAssembly>();
        private Dictionary<TypeReference, IXamlXType> _typeReferenceCache = new Dictionary<TypeReference, IXamlXType>();
        private Dictionary<AssemblyDefinition, CecilAssembly> _assemblyDic 
            = new Dictionary<AssemblyDefinition, CecilAssembly>();
        private Dictionary<string, IXamlXType> _unresolvedTypeCache = new Dictionary<string, IXamlXType>();
        
        private CustomMetadataResolver _resolver;
        private CecilTypeCache _typeCache;
        public void Dispose()
        {
            foreach (var asm in _asms)
                asm.Assembly.Dispose();
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) => ResolveWrapped(name)?.Assembly;
        CecilAssembly ResolveWrapped(AssemblyNameReference name)
        {
            if (_assemblyCache.TryGetValue(name.FullName, out var rv))
                return rv;
            foreach (var asm in _asms)
                if (asm.Assembly.Name.Equals(name))
                    return _assemblyCache[name.FullName] = asm;
            foreach (var asm in _asms)
                if (asm.Assembly.Name.Name == name.Name)
                    return _assemblyCache[name.FullName] = asm;
            throw new AssemblyResolutionException(name);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) => Resolve(name);
        
        public CecilTypeSystem(IEnumerable<string> paths, string targetPath = null)
        {
            if (targetPath != null)
                paths = paths.Concat(new[] {targetPath});
            _resolver = new CustomMetadataResolver(this);
            _typeCache = new CecilTypeCache(this);
            foreach (var path in paths.Distinct())
            {
                var asm = AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Deferred)
                {
                    ReadWrite = path == targetPath,
                    InMemory = true,
                    AssemblyResolver = this,
                    MetadataResolver = _resolver,                    
                });
                RegisterAssembly(asm);
            }    
        }

        public IReadOnlyList<IXamlXAssembly> Assemblies => _asms.AsReadOnly();
        public IXamlXAssembly FindAssembly(string name) => _asms.FirstOrDefault(a => a.Assembly.Name.Name == name);

        public IXamlXType FindType(string name)
        {
            foreach (var asm in _asms)
            {
                var found = asm.FindType(name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public IXamlXType FindType(string name, string assembly) 
            => FindAssembly(assembly)?.FindType(name);

        CecilAssembly FindAsm(AssemblyDefinition d)
        {
            _assemblyDic.TryGetValue(d, out var asm);
            return asm;
        }
        
        IXamlXType Resolve(TypeReference reference)
        {
            if (!_typeReferenceCache.TryGetValue(reference, out var rv))
            {
                var resolved = reference.Resolve();
                
                if (resolved != null)
                {
                    rv = _typeCache.Get(reference);
                }
                else
                {
                    if (!_unresolvedTypeCache.TryGetValue(reference.FullName, out rv))
                        _unresolvedTypeCache[reference.FullName] =
                            rv = new UnresolvedCecilType(reference);
                }
                _typeReferenceCache[reference] = rv;
            }
            return rv;
        }

        public void RegisterAssembly(AssemblyDefinition asm)
        {
            var wrapped = new CecilAssembly(this, asm);
            _asms.Add(wrapped);
            _assemblyDic[asm] = wrapped;
        }
        
        public AssemblyDefinition CreateAndRegisterAssembly(string name, Version version, ModuleKind kind)
        {
            var def = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, version), name,
                new ModuleParameters()
                {
                    AssemblyResolver = this,
                    MetadataResolver = this._resolver,
                    Kind = kind
                });
            RegisterAssembly(def);
            return def;
        }

        private IXamlXMethod Resolve(MethodDefinition method, TypeReference declaringType)
        {
            return new CecilMethod(this, method, declaringType);
        }

        private CecilType GetTypeFor(TypeReference reference) => _typeCache.Get(reference);

        interface ITypeReference
        {
            TypeReference Reference { get; }
        }

        public IXamlXTypeBuilder CreateTypeBuilder(TypeDefinition def)
        {
            return new CecilTypeBuilder(this, FindAsm(def.Module.Assembly), def);
        }

        public AssemblyDefinition GetAssembly(IXamlXAssembly asm)
            => ((CecilAssembly) asm).Assembly;
    }
}
