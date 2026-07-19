import { Injectable, inject } from '@angular/core';
import { LanguageService } from './language.service';
import moment from 'moment-timezone';

@Injectable({ providedIn: 'root' })
export class DateFormatService {
  private langSrv = inject(LanguageService);

  private readonly formats: Record<string, { date: string; dateTime: string; shortDate: string }> = {
    en: {
      date: 'DD/MM/YYYY',
      dateTime: 'DD/MM/YYYY HH:mm',
      shortDate: 'DD/MM/YY',
    },
    ne: {
      date: 'DD/MM/YYYY',
      dateTime: 'DD/MM/YYYY HH:mm',
      shortDate: 'DD/MM/YY',
    },
  };

  get dateFormat(): string {
    const lang = this.langSrv.getLang();
    return this.formats[lang]?.date || 'DD/MM/YYYY';
  }

  get dateTimeFormat(): string {
    const lang = this.langSrv.getLang();
    return this.formats[lang]?.dateTime || 'DD/MM/YYYY HH:mm';
  }

  formatDate(date: string | Date | null | undefined, includeTime = false): string {
    if (!date) return '-';
    const format = includeTime ? this.dateTimeFormat : this.dateFormat;
    return moment(date).format(format);
  }

  formatRelative(date: string | Date | null | undefined): string {
    if (!date) return '-';
    return moment(date).fromNow();
  }

  formatNepal(date: string | Date | null | undefined): string {
    if (!date) return '-';
    return moment(date).tz('Asia/Kathmandu').format('ddd, DD MMM YYYY, hh:mm:ss A');
  }
}