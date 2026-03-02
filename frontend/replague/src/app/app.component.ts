import { Component, OnInit } from '@angular/core';
import { TranslationService } from './modules/i18n';
import { locale as enLang } from './modules/i18n/vocabs/en';
import { locale as esLang } from './modules/i18n/vocabs/es';
import { ThemeModeService } from './_metronic/partials/layout/theme-mode-switcher/theme-mode.service';
import { AppUpdateService } from './core/services/app-update.service';
import { PushService } from './core/services/push.service';
import { InstallPromptService } from './core/services/install-prompt.service';

@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'body[root]',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  constructor(
    private translationService: TranslationService,
    private modeService: ThemeModeService,
    private appUpdate: AppUpdateService,
    private push: PushService,
    // Instantiate to activate beforeinstallprompt listener early
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    private installPrompt: InstallPromptService
  ) {
    this.translationService.loadTranslations(enLang, esLang);
  }

  ngOnInit() {
    this.modeService.init();
    this.appUpdate.init();
    this.push.init();
  }
}
