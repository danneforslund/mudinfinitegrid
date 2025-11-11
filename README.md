# Description

Small repo that extends [MudDataGrid](https://mudblazor.com/components/datagrid#default-data-grid) to allow infinite scrolling without virtualization.

## Installation

Include ```mudinfinitegrid.js``` in your project.

Change 
```razor
<MudDataGrid ... />
```
to 
```razor
<MudInfiniteGrid ... />
```

## Customization

Should you need to control the implementation, just set ```Infinite = false``` and it will behave like a normal MudDataGrid.
