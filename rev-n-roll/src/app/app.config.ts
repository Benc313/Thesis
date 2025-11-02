import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';
import { provideClientHydration } from '@angular/platform-browser';
import { AuthService } from './services/auth.service';
import { UserService } from './services/user.service';
import { CarService } from './services/car.service';
import { MeetService } from './services/meet.service';
import { provideAnimations } from '@angular/platform-browser/animations';
import { RaceService } from './services/race.service';
import { CrewService } from './services/crew.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    provideClientHydration(),
    provideAnimations(),
    AuthService,
    UserService,
    CarService,
    MeetService,
    RaceService,
    CrewService
  ]
};