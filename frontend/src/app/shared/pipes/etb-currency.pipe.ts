import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'etbCurrency',
  standalone: true
})
export class EtbCurrencyPipe implements PipeTransform {
  transform(value: number | string | null | undefined): string {
    if (value === null || value === undefined || value === '') {
      return 'ETB 0';
    }
    const num = typeof value === 'string' ? parseFloat(value) : value;
    if (isNaN(num)) return 'ETB 0';

    return 'ETB ' + num.toLocaleString('en-US');
  }
}
