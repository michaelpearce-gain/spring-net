#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region Imports

using Spring.Context;
using Spring.Validation;

#endregion

namespace Spring.Web.UI
{
    /// <summary>
    /// Abstracts access to properties required for 
    /// handling validation and error rendering
    /// </summary>
    /// <author>Erich Eichinger</author>
    public interface IValidationContainer
    {
        /// <summary>
        /// Gets the MessageSource to be used for 
        /// resolving error ids into messages
        /// </summary>
        IMessageSource MessageSource { get; }
        /// <summary>
        /// Gets the list of validation errors kept by this container.
        /// </summary>
        IValidationErrors ValidationErrors { get; } 
    }
}