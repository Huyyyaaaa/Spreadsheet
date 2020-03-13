// Skeleton implementation written by Joe Zachary for CS 3500, January 2018.
//Sungyeon Han, U0970346 

using System;
using System.Collections.Generic;
using System.Linq;

namespace Dependencies
{
    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// </summary>
    public class DependencyGraph
    {
        //mapping dependent and dependee
        private Dictionary<string, HashSet<string>> dependees;
        private Dictionary<string, HashSet<string>> dependents;

        //pairs count the number of ordered pairs in the graph
        private int pairs;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
            dependees = new Dictionary<string, HashSet<string>>();
            dependents = new Dictionary<string, HashSet<string>>();
            pairs = 0;
        }

        public DependencyGraph(DependencyGraph dg)
        {
            dg = new DependencyGraph();
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return pairs; }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependents(string s)
        {
            //check if any of the parameter is null
            if (s == null) {
                throw new ArgumentNullException("Parameter is null");
            }

            // return true if dependent contains "s"
            //else, return false
            if (dependees.ContainsKey(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependees(string s)
        {
            //check if any of the parameter is null
            if (s == null)
            {
                throw new ArgumentNullException("Parameter is null");
            }

            // return true if dependee contains "s"
            //else, return false 
            if (dependents.ContainsKey(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            //check if any of the parameter is null
            if (s == null)
            {
                throw new ArgumentNullException("Parameter is null");
            }

            //if dependent contains "s", map it to new Hashset
            if (!dependees.ContainsKey(s))
            {
                return new HashSet<string>();
            }
            //else, return empty HashSet
            else
            {
                return new HashSet<string>(dependees[s]);
            }
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            //check if any of the parameter is null
            if (s == null)
            {
                throw new ArgumentNullException("Parameter is null");
            }

            //if dependee contains "s", map it to new Hashset
            if (!dependents.ContainsKey(s))
            {
                return new HashSet<string>();
            }
            //else, return empty HashSet
            else
            {
                return new HashSet<string>(dependents[s]);
            }
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            //check if any of the parameter is null
            if (s == null || t == null)
            {
                throw new ArgumentNullException("Parameter is null");
            }

            //if depdent does not contain strings "s" and "t", 
            //increment pair
            if (!(dependents.ContainsKey(t) && dependees.ContainsKey(s)))
            {
                pairs++;
            }

            if (!dependees.ContainsKey(s))
            {
                HashSet<string> dependents = new HashSet<string>();
                dependents.Add(t);
                dependees.Add(s, dependents);  
            }
            else
            {
                dependees[s].Add(t);
            }

           
            if (!dependents.ContainsKey(t))
            {
                HashSet<string> dependees = new HashSet<string>();
                dependees.Add(s);
                dependents.Add(t, dependees); 
            }
            else 
            {
                dependents[t].Add(s);
            }
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            //check if any of the parameter is null
            if (s == null || t == null)
            {
                throw new ArgumentNullException("Parameter is null");
            }

            //create new HashSet to check if dependents has values
            HashSet<string> getDependents = new HashSet<string>();

            //if dependents and dependees have "s" AND "t" in corresponding getDependents and getDependees,
            //then decrement pairs
            if (dependents.ContainsKey(t) && dependees.ContainsKey(s))
            {
                pairs--;  // decrement the count
            }

            //if dependents has "s" in corresponding getDependents, 
            //remove "t" from corresponding "s" in dependents
            if (dependents.ContainsKey(t))
            {
                dependents[t].Remove(s);
                if (dependents[t].Count == 0)
                {
                    dependents.Remove(t);
                }
            }

            //create new HashSet to check if dependees has values
            HashSet<string> getDependees = new HashSet<string>();

            //if dependees has "s" in corresponding getDependees, 
            //remove "t" from corresponding "s" in dependees
            if (dependees.ContainsKey(s))
            {
                // then just remove 't' from the value HashSet of 's'
                dependees[s].Remove(t);
                if (dependees[s].Count == 0)
                {
                    dependees.Remove(s);
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            //check if any of the parameter is null
            if (s == null || newDependents == null)
            {
                throw new ArgumentNullException("Parameter is null");
            }

            IEnumerable<string> oldDependents = GetDependents(s);

            foreach (string r in oldDependents)
            {
                RemoveDependency(s, r);
            }

            foreach (string t in newDependents)
            {
                AddDependency(s, t);
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            //check if any of the parameter is null
            if (t == null || newDependees == null)
            {
                throw new ArgumentNullException("Parameter is null");
            }

            IEnumerable<string> oldDependees = GetDependees(t);

            foreach (string r in oldDependees)
            {
                RemoveDependency(r, t);
            }

            foreach (string s in newDependees)
            {
                AddDependency(s, t);
            }
        }
    }
}
