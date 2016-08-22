# How to contribute

Contributions are most welcome. The following guide 

## Code of conduct.

This project adheres to the [Open Code of Conduct][code-of-conduct]. By participating, you are expected to honor this code.
[code-of-conduct]: http://todogroup.org/opencodeofconduct/#OneTrueError/support@onetrueerror.com.

Not doing so will get you banned from participating in the project, no exceptions.

## Getting Started

* Make sure you have a [GitHub account](https://github.com/signup/free)
* [Create a new issue](https://github.com/jgauffin/griffin.framework/issues/new), assuming one does not already exist.
  * Clearly describe the issue including steps to reproduce when it is a bug.
  * If it is a bug make sure you tell us what version you have encountered this bug on.
* Fork the repository on GitHub

## Making Changes

* Make commits of logical units.
* Check for unnecessary whitespace with `git diff --check` before committing.
* Make sure your commit messages are in the proper format.
* Make sure you have added the necessary tests for your changes.
* We use Resharper for code style. Make sure that you're "all green in reshaper".

### Unit tests.

There are not that many unit tests yet, however our ultimate goal is to have complete coverage. Therefore all contributions must be covered by tests.

*Guidelines*

* Use Xunit, NSubtitute and FluentAssertions
* Follow the AAA pattern with one empty line as seperator
* Method names should explain the business rule being tested (i.e. "should_not_be_possible_to_login_into_a_locked_account")
* Aim at only one or mostly two asserts per test
* Factory methods should create objects for the working case, modify it after when you test the exceptional cases.

# Additional Resources

* [General GitHub documentation](http://help.github.com/)
* [GitHub pull request documentation](http://help.github.com/send-pull-requests/)