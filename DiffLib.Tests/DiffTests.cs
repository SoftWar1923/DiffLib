﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// ReSharper disable AssignNullToNotNullAttribute

namespace DiffLib.Tests
{
    [TestFixture]
    public class DiffTests
    {
        [Test]
        public void CalculateSections_NullCollection1_ThrowsArgumentNullException()
        {
            List<string> collection1 = null;
            var collection2 = new List<string>();
            EqualityComparer<string> comparer = EqualityComparer<string>.Default;

            Assert.Throws<ArgumentNullException>(() => Diff.CalculateSections(collection1, collection2, comparer));
        }

        [Test]
        public void CalculateSections_NullCollection2_ThrowsArgumentNullException()
        {
            var collection1 = new List<string>();
            List<string> collection2 = null;
            EqualityComparer<string> comparer = EqualityComparer<string>.Default;

            Assert.Throws<ArgumentNullException>(() => Diff.CalculateSections(collection1, collection2, comparer));
        }

        [Test]
        public void AlignElements_NullCollection1_ThrowsArgumentNullException()
        {
            IList<int> collection1 = null;
            IList<int> collection2 = new int[0];
            IEnumerable<DiffSection> diffSections = new DiffSection[0];
            IDiffElementAligner<int> aligner = new BasicInsertDeleteDiffElementAligner<int>();
            Assert.Throws<ArgumentNullException>(() => Diff.AlignElements(collection1, collection2, diffSections, aligner));
        }

        [Test]
        public void AlignElements_NullCollection2_ThrowsArgumentNullException()
        {
            IList<int> collection1 = new int[0];
            IList<int> collection2 = null;
            IEnumerable<DiffSection> diffSections = new DiffSection[0];
            IDiffElementAligner<int> aligner = new BasicInsertDeleteDiffElementAligner<int>();
            Assert.Throws<ArgumentNullException>(() => Diff.AlignElements(collection1, collection2, diffSections, aligner));
        }

        [Test]
        public void AlignElements_NullDiffSections_ThrowsArgumentNullException()
        {
            IList<int> collection1 = new int[0];
            IList<int> collection2 = new int[0];
            IEnumerable<DiffSection> diffSections = null;
            IDiffElementAligner<int> aligner = new BasicInsertDeleteDiffElementAligner<int>();
            Assert.Throws<ArgumentNullException>(() => Diff.AlignElements(collection1, collection2, diffSections, aligner));
        }

        [Test]
        public void AlignElements_NullAligner_ThrowsArgumentNullException()
        {
            IList<int> collection1 = new int[0];
            IList<int> collection2 = new int[0];
            IEnumerable<DiffSection> diffSections = new DiffSection[0];
            IDiffElementAligner<int> aligner = null;
            Assert.Throws<ArgumentNullException>(() => Diff.AlignElements(collection1, collection2, diffSections, aligner));
        }

        [Test]
        public void SimpleDiff_ProducesCorrectResults()
        {
            const string text1 = "This is a test of the diff implementation, with some text that is deleted.";
            const string text2 = "This is another test of the same implementation, with some more text.";

            DiffSection[] diff = Diff.CalculateSections(text1.ToCharArray(), text2.ToCharArray()).ToArray();

            CollectionAssert.AreEqual(diff, new[]
            {
                new DiffSection(true, 9, 9), // same        "This is a"
                new DiffSection(false, 0, 6), // add        "nother"
                new DiffSection(true, 13, 13), // same      " test of the "
                new DiffSection(false, 4, 4), // replace    "same" with "diff"
                new DiffSection(true, 27, 27), // same      " implementation, with some "
                new DiffSection(false, 0, 5), // add        "more "
                new DiffSection(true, 4, 4), // same        "text"
                new DiffSection(false, 16, 0), // delete    " that is deleted"
                new DiffSection(true, 1, 1), // same        "."
            });
        }

        [Test]
        public void Diff_WithNullElements()
        {
            var collection1 = new[]
            {
                "Line 1", "Line 2", null, "Line 3", "Line 4",
            };

            var collection2 = new[]
            {
                "Line 1", null, "Line 2", "Line 4",
            };

            DiffSection[] sections = Diff.CalculateSections(collection1, collection2).ToArray();

            CollectionAssert.AreEqual(sections, new[]
            {
                new DiffSection(true, 1, 1),
                new DiffSection(false, 0, 1),
                new DiffSection(true, 1, 1),
                new DiffSection(false, 2, 0),
                new DiffSection(true, 1, 1),
            });
        }

        [Test]
        public void Align_WithNullElements()
        {
            var collection1 = new[]
            {
                "Line 1", "Line 2", null, "Line 3", "Line 4",
            };

            var collection2 = new[]
            {
                "Line 1", null, "Line 2", "Line 4",
            };

            DiffSection[] sections = Diff.CalculateSections(collection1, collection2).ToArray();
            var elements = Diff.AlignElements(collection1, collection2, sections, new StringSimilarityDiffElementAligner());

            CollectionAssert.AreEqual(new[]
            {
                new DiffElement<string>("Line 1", "Line 1", DiffOperation.Match),
                new DiffElement<string>(Option<string>.None, null, DiffOperation.Insert),
                new DiffElement<string>("Line 2", "Line 2", DiffOperation.Match),
                new DiffElement<string>(null, Option<string>.None, DiffOperation.Delete),
                new DiffElement<string>("Line 3", Option<string>.None, DiffOperation.Delete),
                new DiffElement<string>("Line 4", "Line 4", DiffOperation.Match),
            }, elements);
        }
    }
}