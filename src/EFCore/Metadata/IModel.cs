// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using CA = System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Metadata about the shape of entities, the relationships between them, and how they map to
    ///         the database. A model is typically created by overriding the
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> method on a derived
    ///         <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IModel : IReadOnlyModel, IAnnotatable
    {
        /// <summary>
        ///     Gets the entity with the given name. Returns <see langword="null" /> if no entity type with the given name is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        new IEntityType? FindEntityType([NotNull] string name);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="name"> The name of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IEntityType? FindEntityType(
            [NotNull] string name,
            [NotNull] string definingNavigationName,
            [NotNull] IEntityType definingEntityType);

        /// <summary>
        ///     Gets the entity that maps the given entity class, where the class may be a proxy derived from the
        ///     actual entity type. Returns <see langword="null" /> if no entity type with the given CLR type is found
        ///     or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IEntityType? FindRuntimeEntityType([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return FindEntityType(type)
                ?? (type.BaseType == null
                    ? null
                    : FindEntityType(type.BaseType));
        }

        /// <summary>
        ///     Gets all entity types defined in the model.
        /// </summary>
        /// <returns> All entity types defined in the model. </returns>
        new IEnumerable<IEntityType> GetEntityTypes();

        /// <summary>
        ///     The runtime service dependencies.
        /// </summary>
        [CA.DisallowNull]
        RuntimeModelDependencies? ModelDependencies
        {
            get => (RuntimeModelDependencies?)FindRuntimeAnnotationValue(CoreAnnotationNames.ModelDependencies);
            [param: NotNull]
            set => SetRuntimeAnnotation(CoreAnnotationNames.ModelDependencies, Check.NotNull(value, nameof(value)));
        }

        /// <summary>
        ///     Gets the runtime service dependencies.
        /// </summary>
        RuntimeModelDependencies GetModelDependencies()
        {
            var dependencies = ModelDependencies;
            if (dependencies == null)
            {
                throw new InvalidOperationException(CoreStrings.ModelNotFinalized(nameof(GetModelDependencies)));
            }

            return dependencies;
        }

        /// <summary>
        ///     Gets the entity that maps the given entity class. Returns <see langword="null" /> if no entity type with
        ///     the given CLR type is found or the given CLR type is being used by shared type entity type
        ///     or the entity type has a defining navigation.
        /// </summary>
        /// <param name="type"> The type to find the corresponding entity type for. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        new IEntityType? FindEntityType([NotNull] Type type);

        /// <summary>
        ///     Gets the entity type for the given name, defining navigation name
        ///     and the defining entity type. Returns <see langword="null" /> if no matching entity type is found.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <param name="definingNavigationName"> The defining navigation of the entity type to find. </param>
        /// <param name="definingEntityType"> The defining entity type of the entity type to find. </param>
        /// <returns> The entity type, or <see langword="null" /> if none is found. </returns>
        IEntityType? FindEntityType(
            [NotNull] Type type,
            [NotNull] string definingNavigationName,
            [NotNull] IEntityType definingEntityType)
            => (IEntityType?)((IReadOnlyModel)this).FindEntityType(type, definingNavigationName, definingEntityType);

        /// <summary>
        ///     Gets the entity types matching the given type.
        /// </summary>
        /// <param name="type"> The type of the entity type to find. </param>
        /// <returns> The entity types found. </returns>
        [DebuggerStepThrough]
        new IEnumerable<IEntityType> FindEntityTypes([NotNull] Type type);

        /// <summary>
        ///     Returns the entity types corresponding to the least derived types from the given.
        /// </summary>
        /// <param name="type"> The base type. </param>
        /// <param name="condition"> An optional condition for filtering entity types. </param>
        /// <returns> List of entity types corresponding to the least derived types from the given. </returns>
        new IEnumerable<IEntityType> FindLeastDerivedEntityTypes(
            [NotNull] Type type,
            [CanBeNull] Func<IReadOnlyEntityType, bool>? condition = null)
            => ((IReadOnlyModel)this).FindLeastDerivedEntityTypes(type, condition == null ? null : t => condition(t))
                .Cast<IEntityType>();

        /// <summary>
        ///     Gets a value indicating whether the given <see cref="MethodInfo"/> reprensents an indexer access.
        /// </summary>
        /// <param name="methodInfo"> The <see cref="MethodInfo"/> to check. </param>
        bool IsIndexerMethod([NotNull] MethodInfo methodInfo);
    }
}
