using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using System.Collections.Generic;
using Bitub.Transfer.Tests;

namespace Bitub.Transfer.Scene.Tests
{
    [TestClass]
    public class FacetMeshTests : BaseTest<FacetMeshTests>
    {
        string jsonPointArray = "{ \"xyz\": [ 0, 0, 2.5, 2.25, 0, 0, 2.25, 0, 2.5, 0, 0, 0, 2.25, 0.25, 0, 2.25, 0.25, 2.5, 0, 0.25, 2.5, 0, 0.25, 0 ] }";
        string jsonBody = "{ \"isShell\": false, \"isConvex\": false, \"material\": { \"nid\": 104 }, \"faces\": [ { \"orient\": \"UNKNOWN\", \"loop\": [ ], \"uv\": [ ], \"isPlanar\": true, \"mesh\": { \"type\": \"TRI_MESH\", \"orient\": \"CCW\", \"vertex\": [ 0, 1, 2, 1, 0, 3 ], \"normal\": [ -2.99932144E-32, -1, 1.22460635E-16 ], \"uv\": [ ] } }, { \"orient\": \"UNKNOWN\", \"loop\": [ ], \"uv\": [ ], \"isPlanar\": true, \"mesh\": { \"type\": \"TRI_MESH\", \"orient\": \"CCW\", \"vertex\": [ 2, 4, 5, 4, 2, 1 ], \"normal\": [ 1, 0, 0 ], \"uv\": [ ] } }, { \"orient\": \"UNKNOWN\", \"loop\": [ ], \"uv\": [ ], \"isPlanar\": true, \"mesh\": { \"type\": \"TRI_MESH\", \"orient\": \"CCW\", \"vertex\": [ 4, 6, 5, 6, 4, 7 ], \"normal\": [ 0, 1, 0 ], \"uv\": [ ] } }, { \"orient\": \"UNKNOWN\", \"loop\": [ ], \"uv\": [ ], \"isPlanar\": true, \"mesh\": { \"type\": \"TRI_MESH\", \"orient\": \"CCW\", \"vertex\": [ 7, 0, 6, 0, 7, 3 ], \"normal\": [ -1, 6.123032E-17, -1.83690953E-16 ], \"uv\": [ ] } }, { \"orient\": \"UNKNOWN\", \"loop\": [ ], \"uv\": [ ], \"isPlanar\": true, \"mesh\": { \"type\": \"TRI_MESH\", \"orient\": \"CCW\", \"vertex\": [ 1, 7, 4, 7, 1, 3 ], \"normal\": [ 1.22460635E-16, 6.123032E-17, -1 ], \"uv\": [ ] } }, { \"orient\": \"UNKNOWN\", \"loop\": [ ], \"uv\": [ ], \"isPlanar\": true, \"mesh\": { \"type\": \"TRI_MESH\", \"orient\": \"CCW\", \"vertex\": [ 6, 2, 5, 2, 6, 0 ], \"normal\": [ 0, 0, 1 ], \"uv\": [ ] } } ] }";

        const uint SHIFT = 1000;

        [TestInitialize]
        public void StartUp()
        {
            base.StartUpLogging();
        }

        private void TestRunWith(PtArray ptArray, FaceBody body, FacetStarVisitor.InvestigationStrategy strategy)
        {
            var facetStar = new FacetStars(body, new PtOffsetArray(ptArray, SHIFT));

            Assert.AreEqual(8, facetStar.Vertices.Count(), "8 vertices");
            Assert.AreEqual(8, facetStar.Count, "8 stars");
            // Use no visitor's index cache
            var visitor = new FacetStarVisitor(facetStar, (i) => false, strategy);
            var faces = new List<MeshPtOffsetArray>();
            var facets = new HashSet<Facet>();
            foreach (Facet f in visitor)
            {
                switch (visitor.Strategy)
                {
                    case FacetStarVisitor.InvestigationStrategy.SameFaceFirst:
                        if (!f.Meshed.Equals(faces.LastOrDefault()))
                        {
                            foreach (var known in faces.Take(faces.Count - 1))
                                Assert.AreNotEqual(known, f.Meshed, "Face shouldn't be processed before");

                            faces.Add(f.Meshed);
                            Assert.IsTrue(visitor.IsNewFace);
                        }
                        else
                        {
                            Assert.IsFalse(visitor.IsNewFace);
                        }
                        break;
                }

                facets.Add(f);
                Assert.AreEqual(SHIFT, f.Shift);
                Assert.IsTrue(f.IsTriangle(), "Is Triangle");
                Assert.IsTrue(f.IsValid(), "Is Valid");
            }

            Assert.IsFalse(visitor.HasNextCandidate, "All facets have been processed");
            Assert.AreEqual(12, facets.Count, "12 facets");
        }

        [TestMethod]
        public void EnumerateFacetStarsFirstWins()
        {
            var ptArray = PtArray.Parser.ParseJson(jsonPointArray);
            var body = FaceBody.Parser.ParseJson(jsonBody);
            TestRunWith(ptArray, body, FacetStarVisitor.InvestigationStrategy.FirstWins);
        }

        [TestMethod]
        public void EnumerateFacetStarsSameFaceFirst()
        {
            var ptArray = PtArray.Parser.ParseJson(jsonPointArray);
            var body = FaceBody.Parser.ParseJson(jsonBody);
            TestRunWith(ptArray, body, FacetStarVisitor.InvestigationStrategy.SameFaceFirst);
        }

    }
}
