﻿// OsmSharp - OpenStreetMap tools & library.
// Copyright (C) 2012 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Osm.Simple;

namespace OsmSharp.Osm.Data.Core.Processor.ChangeSets
{
    /// <summary>
    /// A changeset filter.
    /// </summary>
    public abstract class DataProcessorChangeSetFilter : DataProcessorChangeSetSource
    {
        /// <summary>
        /// The source.
        /// </summary>
        private DataProcessorChangeSetSource _source;

        /// <summary>
        /// Creates a new data processor changeset filter.
        /// </summary>
        protected DataProcessorChangeSetFilter()
        {

        }

        /// <summary>
        /// The source this filter is filtering from.
        /// </summary>
        protected DataProcessorChangeSetSource Source
        {
            get
            {
                return _source;
            }
        }

        /// <summary>
        /// Initializes the filter.
        /// </summary>
        public abstract override void Initialize();

        /// <summary>
        /// Moves to the next changeset.
        /// </summary>
        /// <returns></returns>
        public abstract override bool MoveNext();

        /// <summary>
        /// Returns the current changeset.
        /// </summary>
        /// <returns></returns>
        public abstract override SimpleChangeSet Current();

        /// <summary>
        /// Registers a changeset source.
        /// </summary>
        /// <param name="source"></param>
        public void RegisterSource(DataProcessorChangeSetSource source)
        {
            _source = source;
        }
    }
}
