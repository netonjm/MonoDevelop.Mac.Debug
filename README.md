# MonoDevelop.Mac.Debug

A set of debugging tools to help track down Cocoa issues related to first responders and visual tree layout.

After ViewDebuggerContext.Attach a set of four new menus will be added to your main application menu:

- Show KeyViewLoop Debug Window
- Show First Responder Overlay
- Show Next Responder Overlay
- Show Previous Responder Overlay

## Usage

- Build MonoDevelop.Mac.Debug
- Reference the assembly in Xamarin.Mac application
- ViewDebuggerContext.Attach (window);

<img width="1488" alt="image" src="https://user-images.githubusercontent.com/1587480/163312075-3e4b7883-8f7e-4f60-915a-d7c5b040ba5a.png">

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## License

MIT
