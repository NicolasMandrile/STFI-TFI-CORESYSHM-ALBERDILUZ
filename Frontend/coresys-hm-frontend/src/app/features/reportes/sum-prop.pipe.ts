import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'sumProp', standalone: false })
export class SumPropPipe implements PipeTransform {
  transform(items: any[], prop: string): number {
    return items.reduce((acc, item) => acc + (item[prop] ?? 0), 0);
  }
}
