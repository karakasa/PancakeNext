# Pancake for Grasshopper 2

A open-source port of [Pancake](https://www.food4rhino.com/en/app/pancake) on GH2.

Please be advised that Pancake itself has abstraction layers to handle localization, etc. So the code would look different from a vanilla GH2 component.

# Handbook of Porting

[Wiki](https://github.com/karakasa/PancakeNext/wiki). The wiki includes a quick starter guide and covers several other topics like how GH'2 new data model works.

# Example: 1 shared codebase for 2 GHs

[Codebase](/example/OneCodeTwoVersions)

It utilizes project configurations, preprocess directives and a thin compatibility layer for sharing codebases across GH1 & 2. The compatibility middleware focuses on the original GH1 syntax, making it runnable under GH2 with only minor changes.

The project is merely for technical demonstration purpose. The API surface and interoperability implementation is subject to Grasshopper 1 terms by McNeel and David Rutten.

# License

[Apache-2.0](LICENSE.txt)

In short:
* No warranty.
* Free for both non-commercial and commercial use.
* Attribution required in modified files, if you make derivative works of this repo.

The example project above has its own license terms.
