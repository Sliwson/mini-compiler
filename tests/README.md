# Testing tools for *mini* and *mini++* language compilers (see: [tasks](../doc))

### Test structure

Files `tests.txt`, `tests_groupA.txt`, etc. contain lists of test files.
	
Source files listed in e.g. `tests.txt` should be located in the `tests` folder. Expected results should be located in `expected_results` and have exactly the same names as files in `tests`. The `inputs` directory should contain the expected input file, if needed.
	
In case of an expected compilation error, the corresponding file in the `expected_results` folder should contain the text "error" – examples already exist.
	
### How to test
	
`.bat` files are used for testing. Run the `test.bat` file (from VS CMD, from the folder where this file is located), where the first argument **must** be the path to the executable file of our *mini* language compiler.
	
These files save results in the `results` folder. The message "critical_error" in a file indicates an error in one of the three stages that occur after our compiler passes (CIL code assembly, verification by peverify program, program execution). The message "compiler_error" indicates a negative exit code from our compiler – this most likely refers to an uncaught exception. Make sure to return a positive value in case of compilation error.

Outputs from `results` are compared with corresponding files in `expected_results`. All found differences are saved to the `errors.txt` file. If it contains only the line "errors_detected" - it means everything is OK.

### Notes

The `test_for5.bat` test, in addition to tests from `tests_for5.txt`, also aggregates all other test groups.
	
The compiler itself cannot contain calls to `Console.ReadKey` or `Console.ReadLine` methods – waiting for user input is unnecessary and treated as compiler hanging.

People developing their compiler on Unix systems can use the `test.sh` script which is the Bash equivalent of the `test.bat` script.
