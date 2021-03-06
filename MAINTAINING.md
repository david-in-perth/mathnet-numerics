Maintaining Math.NET Numerics
=============================

*Note: This document is only relevant for the maintainers of this project*

When creating a new release
---------------------------

Repository:

- Update RELEASENOTES file with relevant changes, attributed by contributor (if external). Set date.
- Update CONTRIBUTORS file (via `git shortlog -sn`)
- Consider to update the repository mirrors at codeplex, gitorious and google ([how to](http://christoph.ruegg.name/blog/2013/1/26/git-howto-mirror-a-github-repository-without-pull-refs.html)).

Publish:

- Upload NuGet packages to the NuGet Gallery
- Create new Codeplex and GitHub release, attach Zip files

Misc:

- Consider a tweet via [@MathDotNet](https://twitter.com/MathDotNet)
- Consider a post to the [Google+ site](https://plus.google.com/112484567926928665204)
- Update Wikipedia release version+date for the [Math.NET Numerics](http://en.wikipedia.org/wiki/Math.NET_Numerics) and [Comparison of numerical analysis software](http://en.wikipedia.org/wiki/Comparison_of_numerical_analysis_software) articles.
- Consider blog post about changes
