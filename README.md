# Mini Compiler

A compiler for the "mini" programming language that generates LLVM IR code.

## Description

This compiler translates programs written in the mini language into LLVM IR (.ll files). The mini language supports:

- Basic data types: `int`, `double`, `bool`
- Variable declarations and assignments
- Input/output operations (`read`, `write`)
- Control flow statements (`if`/`else`, `while`)
- Arithmetic and logical expressions
- Hexadecimal I/O for integers

## Building

Prerequisites:
- .NET Framework 4.0 or later
- Visual Studio or MSBuild

To build the project:
1. Open `mini-compiler.sln` in Visual Studio and build, or
2. Use MSBuild: `msbuild mini-compiler.sln`

The project includes pre-build events that generate parser and scanner files using GPLEX and GPPG tools.

## Usage

Compile a mini program:
```bash
mini-compiler.exe program.mini
```

This generates `program.mini.ll` containing LLVM IR code.

## Example Program

```mini
program
{
    int i;
    double d;
    
    i = 42;
    d = 3.14;
    
    write "Integer: ";
    write i;
    write "\nDouble: ";
    write d;
    write "\n";
}
```

## Testing

See [tests/README.md](tests/README.md) for comprehensive testing instructions.

## License

This project includes third-party libraries:
- GPLEX (Gardens Point Scanner Generator)
- GPPG (Gardens Point Parser Generator)